namespace MedMatch.Verify;

/// <summary>Bağımlılıksız minik test koşucusu. Başarısız olursa süreç non-zero döner.</summary>
public static class Check
{
    private static int _passed;
    private static int _failed;

    public static void True(string name, bool condition)
    {
        if (condition) { _passed++; Console.WriteLine($"  PASS  {name}"); }
        else { _failed++; Console.WriteLine($"  FAIL  {name}"); }
    }

    public static void Throws<TException>(string name, Action action) where TException : Exception
    {
        try { action(); _failed++; Console.WriteLine($"  FAIL  {name} (istisna beklendi, atılmadı)"); }
        catch (TException) { _passed++; Console.WriteLine($"  PASS  {name}"); }
        catch (Exception ex) { _failed++; Console.WriteLine($"  FAIL  {name} (beklenen {typeof(TException).Name}, gelen {ex.GetType().Name})"); }
    }

    public static int Summary()
    {
        Console.WriteLine();
        Console.WriteLine($"Sonuç: {_passed} geçti, {_failed} kaldı.");
        return _failed == 0 ? 0 : 1;
    }
}
