namespace Vm_Serial;

public class Worker : BackgroundService {
    private readonly ILogger<Worker> _logger;
    private readonly SerialService _serialService;
    private readonly OutputHandler _outputHandler;

    public Worker(ILogger<Worker> logger, SerialService serial, OutputHandler output) {
        _logger = logger;
        _serialService = serial;
        _outputHandler = output;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            _logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);

            if (!_serialService.HasPort()) {
                _serialService.GetPort();
            }

            await _serialService.WaitRead(stoppingToken);
            byte[] data = _serialService.ReadSerial();
            try {
                _outputHandler.Write(data);
                _logger.LogDebug("Data: {data}", data);
            } catch (IOException e) {
                if (e.Message.Contains("Pipe is broken")) {
                    _logger.LogWarning("Restarting pipe connection...");
                    _outputHandler.getNewPipe();
                    _logger.LogWarning("Reconnected");
                }
            }


            // await Task.Delay(10);
            _serialService.Discard();
        }
    }
}
