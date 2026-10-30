using System.Diagnostics;
namespace TaskCoordinator;
public sealed class DiagnosticService
{
    public async Task<DiagnosticResult> RunAsync(string name, TimeSpan duration, CancellationToken token = default)
    {
        // TODO 1: Проверить имя и неотрицательную длительность.
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }
        if (duration < TimeSpan.FromMilliseconds(0))
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }
        var sw = Stopwatch.StartNew();
        // TODO 2: Передать token в Task.Delay.
        await Task.Delay(duration, token);
        sw.Stop();
        // TODO 3: Вернуть статус Completed и длительность.
        return new(name, DiagnosticStatus.Completed, "", sw.Elapsed);
    }
    public async Task<DiagnosticResult> RunCpuBoundAsync(string name, int iterations, CancellationToken token = default)
    {
        // TODO 4: Перенести CPU-bound цикл в Task.Run; регулярно проверять token.
        long sum = await Task.Run(() =>
        {
            long s = 0;
            for (int i = 0; i < iterations; i++)
            {
                if (i % 1000 == 0) token.ThrowIfCancellationRequested();
                s += i % 7;
            }
            return s;
        }, token);
        await Task.Yield(); 
        return new(name, DiagnosticStatus.Completed, $"Сумма: {sum}", TimeSpan.Zero);
    }
    public async Task<IReadOnlyList<DiagnosticResult>> RunAllAsync(CancellationToken token = default)
    {
        var tasks = new[] { RunAsync("Сеть", TimeSpan.FromMilliseconds(600), token), RunAsync("Диск", TimeSpan.FromMilliseconds(900), token), RunAsync("База", TimeSpan.FromMilliseconds(700), token) };
        // TODO 5: Дождаться всех задач через WhenAll.
        return await Task.WhenAll(tasks);
    }
    public async Task<DiagnosticResult> RunWithTimeoutAsync(string name, TimeSpan duration, TimeSpan timeout, CancellationToken token = default)
    {
        // TODO 6: Создать linked CancellationTokenSource на основе внешнего token.
        var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        // TODO 7: Настроить CancelAfter(timeout).
        cts.CancelAfter(timeout);
        // TODO 8: Отличить тайм-аут от отмены вызывающей стороны; тайм-аут сообщать TimeoutException.
        return await RunAsync(name, duration, cts.Token);
    }
    public async Task<string> FirstFinishedAsync(CancellationToken token = default)
    {
        var tasks = new[] { RunAsync("Быстрая", TimeSpan.FromMilliseconds(150), token), RunAsync("Средняя", TimeSpan.FromMilliseconds(350), token), RunAsync("Медленная", TimeSpan.FromMilliseconds(600), token) };
        // TODO 9: Использовать WhenAny, затем await первой задачи.
        var firstCompleted = await Task.WhenAny(tasks);
        var result = await firstCompleted;
        return result.Name;
    }
    // TODO 10: Добавить обёртку, которая возвращает статусы Failed/Cancelled
    // и текст ошибки вместо сокрытия исключений.
}
