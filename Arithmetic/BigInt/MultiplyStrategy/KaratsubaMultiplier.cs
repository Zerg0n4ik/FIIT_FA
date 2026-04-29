using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class KaratsubaMultiplier : IMultiplier
{
    private const int Threshold = 32;

    private readonly SimpleMultiplier _simple = new();

    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        if (IsZero(a) || IsZero(b))
            return BetterBigInteger.Zero;

        bool resultNegative = a.IsNegative ^ b.IsNegative;

        var aDigits = a.GetDigits();
        var bDigits = b.GetDigits();

        uint[] product = KaratsubaMultiply(aDigits, bDigits);
        return new BetterBigInteger(product, resultNegative);
    }

    private static bool IsZero(BetterBigInteger value)
    {
        var digits = value.GetDigits();
        return digits.Length == 1 && digits[0] == 0;
    }

    private uint[] KaratsubaMultiply(ReadOnlySpan<uint> x, ReadOnlySpan<uint> y)
    {
        y = DeleteZeros(y);

        int n = Math.Max(x.Length, y.Length);

        if (n <= Threshold)
        {
            return SimpleMultiplier.MultiplyArrays(x.ToArray(), y.ToArray());
        }

        int m = (n + 1) / 2;

        ReadOnlySpan<uint> x0 = x.Slice(0, Math.Min(m, x.Length));
        ReadOnlySpan<uint> x1 = x.Length > m ? x.Slice(m) : default;
        ReadOnlySpan<uint> y0 = y.Slice(0, Math.Min(m, y.Length));
        ReadOnlySpan<uint> y1 = y.Length > m ? y.Slice(m) : default;

        uint[] sumX = Add(x0, x1);
        uint[] sumY = Add(y0, y1);

        uint[] z0 = KaratsubaMultiply(x0, y0);
        uint[] z2 = KaratsubaMultiply(x1, y1);
        uint[] z1 = KaratsubaMultiply(sumX, sumY);

        z1 = Subtract(z1, z0);
        z1 = Subtract(z1, z2);

        return LastSum(z0, z1, z2, m);
    }

    private static uint[] Add(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        int maxLen = Math.Max(a.Length, b.Length);
        var result = new uint[maxLen + 1];
        ulong carry = 0;
        for (int i = 0; i < maxLen; i++)
        {
            ulong sum = carry;
            if (i < a.Length) sum += a[i];
            if (i < b.Length) sum += b[i];
            result[i] = (uint)sum;
            carry = sum >> 32;
        }
        if (carry != 0)
            result[maxLen] = (uint)carry;
        else
            Array.Resize(ref result, maxLen);
        return result;
    }

    private static void AddWithShift(uint[] dest, uint[] source, int shift)
    {
        ulong carry = 0;
        for (int i = 0; i < source.Length; i++)
        {
            int idx = i + shift;
            ulong sum = dest[idx] + (ulong)source[i] + carry;
            dest[idx] = (uint)sum;
            carry = sum >> 32;
        }
        int k = source.Length + shift;
        while (carry != 0 && k < dest.Length)
        {
            ulong sum = dest[k] + carry;
            dest[k] = (uint)sum;
            carry = sum >> 32;
            k++;
        }
    }
    private static uint[] Subtract(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        var result = new uint[a.Length];
        long borrow = 0;
        for (int i = 0; i < a.Length; i++)
        {
            long diff = a[i] - borrow;
            if (i < b.Length) diff -= b[i];
            if (diff < 0)
            {
                diff += 0x100000000;
                borrow = 1;
            }
            else
                borrow = 0;
            result[i] = (uint)diff;
        }
        int last = result.Length - 1;
        while (last >= 0 && result[last] == 0)
            last--;
        if (last < 0) return new uint[] { 0 };
        if (last < result.Length - 1)
            Array.Resize(ref result, last + 1);
        return result;
    }

    private static ReadOnlySpan<uint> DeleteZeros(ReadOnlySpan<uint> span)
    {
        int last = span.Length - 1;
        while (last >= 0 && span[last] == 0)
            last--;
        return last < 0 ? default : span.Slice(0, last + 1);
    }
    private static uint[] LastSum(uint[] z0, uint[] z1, uint[] z2, int m)
    {
        int totalLen = Math.Max(z0.Length, Math.Max(z1.Length + m, z2.Length + 2 * m));
        var result = new uint[totalLen];

        Array.Copy(z0, 0, result, 0, z0.Length);

        AddWithShift(result, z1, m);

        AddWithShift(result, z2, 2 * m);

        int last = result.Length - 1;
        while (last >= 0 && result[last] == 0)
            last--;
        if (last < 0) return new uint[] { 0 };
        if (last < result.Length - 1)
            Array.Resize(ref result, last + 1);
        return result;
    }

}