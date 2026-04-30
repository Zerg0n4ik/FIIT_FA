using System.Numerics;
using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class FftMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger x, BetterBigInteger y)
    {
        if (IsZero(x) || IsZero(y))
            return BetterBigInteger.Zero;

        bool resultSign = x.IsNegative ^ y.IsNegative;
        var digitsOfX = x.GetDigits();
        var digitsOfY = y.GetDigits();

        int xIn16 = digitsOfX.Length * 2;
        int yIn16 = digitsOfY.Length * 2;
        int convLen = xIn16 + yIn16 - 1;
        int size = 1;
        while (size < convLen)
            size <<= 1;

        Complex[] X = new Complex[size];
        Complex[] Y = new Complex[size];

        for (int i = 0; i < digitsOfX.Length; i++)
        {
            uint d = digitsOfX[i];
            X[i * 2] = new Complex(d & 0xFFFF, 0);
            X[i * 2 + 1] = new Complex(d >> 16, 0);
        }
        for (int i = 0; i < digitsOfY.Length; i++)
        {
            uint d = digitsOfY[i];
            Y[i * 2] = new Complex(d & 0xFFFF, 0);
            Y[i * 2 + 1] = new Complex(d >> 16, 0);
        }

        FFT(X, false);
        FFT(Y, false);
        for (int i = 0; i < size; i++)
            X[i] *= Y[i];
        FFT(X, true);
        // округление и перенос
        long carry = 0;
        var digits16 = new List<ushort>();
        for (int i = 0; i < convLen; i++)
        {
            long val = (long)Math.Round(X[i].Real) + carry;
            digits16.Add((ushort)(val & 0xFFFF));
            carry = val >> 16;
        }
        while (carry > 0)
        {
            digits16.Add((ushort)(carry & 0xFFFF));
            carry >>= 16;
        }
        // обратно в 32 бита
        var digits32 = new List<uint>();
        for (int i = 0; i < digits16.Count; i += 2)
        {
            uint low = digits16[i];
            uint high = (i + 1 < digits16.Count) ? digits16[i + 1] : 0u;
            digits32.Add(low | (high << 16));
        }

        while (digits32.Count > 1 && digits32[^1] == 0)
            digits32.RemoveAt(digits32.Count - 1);

        return new BetterBigInteger(digits32, resultSign);
    }

    private static void FFT(Complex[] data, bool inverse)
    {
        int n = data.Length;
        // инвертируем индексы
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; j >= bit; bit >>= 1)
                j -= bit;
            j += bit;
            if (i < j)
                (data[i], data[j]) = (data[j], data[i]);
        }

        for (int len = 2; len <= n; len <<= 1)
        {
            double angle = 2 * Math.PI / len * (inverse ? -1 : 1);
            Complex wBase = new Complex(Math.Cos(angle), Math.Sin(angle));
            // бабочка
            for (int i = 0; i < n; i += len)
            {
                Complex w = Complex.One;
                for (int j = 0; j < len / 2; j++)
                {
                    Complex u = data[i + j];
                    Complex v = data[i + j + len / 2] * w;
                    data[i + j] = u + v;
                    data[i + j + len / 2] = u - v;
                    w *= wBase;
                }
            }
        }

        if (inverse)
        {
            for (int i = 0; i < n; i++)
                data[i] /= n;
        }
    }

    private static bool IsZero(BetterBigInteger x)
    {
        var d = x.GetDigits();
        return d.Length == 1 && d[0] == 0;
    }
}