using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Godot;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Parsing;

namespace OpenCCG.Core.Serilog;

public class GodotSink : ILogEventSink
{
    private readonly ITextFormatter _formatter;

    public GodotSink(string outputTemplate, IFormatProvider? formatProvider)
    {
        _formatter = new TemplateRenderer(outputTemplate, formatProvider);
    }

    public void Emit(LogEvent logEvent)
    {
        using TextWriter writer = new StringWriter();
        _formatter.Format(logEvent, writer);
        writer.Flush();

        foreach (var line in writer.ToString()?.Split('\n') ?? Array.Empty<string>()) GD.PrintRich($"{line}");

        if (logEvent.Exception is null) return;

        if (logEvent.Level >= LogEventLevel.Error)
            GD.PushError(logEvent.Exception);
        else
            GD.PushWarning(logEvent.Exception);
    }

    private class TemplateRenderer : ITextFormatter
    {
        private readonly IFormatProvider? _formatProvider;

        private readonly Renderer[] _renderers;

        public TemplateRenderer(string outputTemplate, IFormatProvider? formatProvider)
        {
            _formatProvider = formatProvider;

            var template = new MessageTemplateParser().Parse(outputTemplate);
            _renderers = template.Tokens.Select(
                token => token switch
                {
                    TextToken textToken => (_, output) => output.Write(textToken.Text),
                    PropertyToken propertyToken => propertyToken.PropertyName switch
                    {
                        OutputProperties.LevelPropertyName
                            => (logEvent, output) => output.Write(logEvent.Level),
                        OutputProperties.MessagePropertyName
                            => (logEvent, output) => logEvent.RenderMessage(output, formatProvider),
                        OutputProperties.NewLinePropertyName
                            => (_, output) => output.Write('\n'),
                        OutputProperties.TimestampPropertyName
                            => RenderTimestamp(propertyToken.Format),
                        _
                            => RenderProperty(propertyToken.PropertyName, propertyToken.Format)
                    },
                    _ => null
                }
            ).OfType<Renderer>().ToArray();
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            foreach (var renderer in _renderers)
                renderer.Invoke(logEvent, output);
        }

        private Renderer RenderTimestamp(string? format)
        {
            Func<LogEvent, string> f =
                _formatProvider?.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter formatter
                    ? logEvent => formatter.Format(format, logEvent.Timestamp, _formatProvider)
                    : logEvent =>
                        logEvent.Timestamp.ToString(format, _formatProvider ?? CultureInfo.InvariantCulture);

            return (logEvent, output) => output.Write(f(logEvent));
        }

        private Renderer RenderProperty(string propertyName, string? format)
        {
            return delegate(LogEvent logEvent, TextWriter output)
            {
                if (logEvent.Properties.TryGetValue(propertyName, out var propertyValue))
                    propertyValue.Render(output, format, _formatProvider);
            };
        }

        private delegate void Renderer(LogEvent logEvent, TextWriter output);
    }
}

public static class GodotSinkExtensions
{
    private const string DefaultGodotSinkOutputTemplate = "[{Timestamp:HH:mm:ss}] {Message:lj}";

    public static LoggerConfiguration Godot(this LoggerSinkConfiguration configuration,
        string outputTemplate = DefaultGodotSinkOutputTemplate,
        IFormatProvider? formatProvider = null)
    {
        return configuration.Sink(new GodotSink(outputTemplate, formatProvider));
    }
}