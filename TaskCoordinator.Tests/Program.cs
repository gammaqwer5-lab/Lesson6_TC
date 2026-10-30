using TaskCoordinator;
var pass = 0;
var fail = 0;
void Check(bool ok, string n)
{
    if (ok)
    {
        pass++;
        Console.WriteLine("PASS " + n);
    }
    else
    {
        fail++;
        Console.WriteLine("FAIL " + n);
    }
}
var s = new DiagnosticService();
var one = await s.RunAsync("Сеть", TimeSpan.FromMilliseconds(20));
Check(one.Status == DiagnosticStatus.Completed, "одна диагностика завершается");
var all = await s.RunAllAsync();
Check(
    all.Count == 3 && all.All(x => x.Status == DiagnosticStatus.Completed),
    "WhenAll ждёт три диагностики"
);
var first = await s.FirstFinishedAsync();
Check(first == "Быстрая", "WhenAny возвращает первую завершившуюся");
// TODO: Добавить тесты кооперативной отмены, тайм-аута, неверного имени и отрицательного времени.

var ioCancelThrown = false;
try
{
    using var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromMilliseconds(50));
    await s.RunAsync("тест", TimeSpan.FromMilliseconds(2000), cts.Token);
}
catch (OperationCanceledException)
{
    ioCancelThrown = true;
}
Check(ioCancelThrown, "кооперативная отмена");


var invalidNameThrown = false;
try
{
    await s.RunAsync("", TimeSpan.FromMilliseconds(10));
}
catch (ArgumentException)
{
    invalidNameThrown = true;
}
Check(invalidNameThrown, "пустое имя");

var negativeTimeThrown = false;
try
{
    await s.RunAsync("тест", TimeSpan.FromMilliseconds(-10));
}
catch (ArgumentOutOfRangeException)
{
    negativeTimeThrown = true;
}
Check(negativeTimeThrown, "отрицательное время");

Console.WriteLine($"Итого: {pass} пройдено, {fail} ошибок.");
return fail == 0 ? 0 : 1;
