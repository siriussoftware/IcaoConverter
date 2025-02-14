﻿internal class ICAOConverter
{
    private const int ICAO_SIZE = 6; // size of an icao address
    private const int NNUMBER_MAX_SIZE = 6; // max size of a N-Number

    private static readonly string charset = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // alphabet without I and O
    private static readonly string digitset = "0123456789";
    private static readonly string hexset = "0123456789ABCDEF";
    private static readonly string allchars = charset + digitset;

    private static readonly int suffix_size = 1 + charset.Length * (1 + charset.Length); // 601
    private static readonly int bucket4_size = 1 + charset.Length + digitset.Length; // 35
    private static readonly int bucket3_size = digitset.Length * bucket4_size + suffix_size; // 951
    private static readonly int bucket2_size = digitset.Length * bucket3_size + suffix_size; // 10111
    private static readonly int bucket1_size = digitset.Length * bucket2_size + suffix_size; // 101711


    private string get_suffix(int offset)
    {
        if (offset == 0) return string.Empty;
        var char0 = charset[(offset - 1) / (charset.Length + 1)];
        var rem = (offset - 1) % (charset.Length + 1);
        if (rem == 0) return char0.ToString();
        return char0 + charset[rem - 1].ToString();
    }

    private int? suffix_offset(string s)
    {
        if (s.Length == 0) return 0;
        var valid = true;
        if (s.Length > 2)
            valid = false;
        else
            foreach (var c in s)
                if (!charset.Contains(c))
                {
                    valid = false;
                    break;
                }

        if (!valid)
        {
            Console.WriteLine("parameter of suffix_shift() invalid");
            Console.WriteLine(s);
            return null;
        }

        var count = (charset.Length + 1) * charset.IndexOf(s[0]) + 1;
        if (s.Length == 2) count += charset.IndexOf(s[1]) + 1;
        return count;
    }

    private string CreateIcao(string prefix, int i)
    {
        var suffix = i.ToString("X").ToLower(); // Convert to hexadecimal
        var l = prefix.Length + suffix.Length;
        if (l > ICAO_SIZE) return null;
        return prefix + new string('0', ICAO_SIZE - l) + suffix;
    }

    public string IcaoToN(string icao)
    {
        // check parameter validity
        icao = icao.ToUpper();
        var valid = true;
        if (icao.Length != ICAO_SIZE || icao[0] != 'A')
            valid = false;
        else
            foreach (var c in icao)
                if (!hexset.Contains(c))
                {
                    valid = false;
                    break;
                }

        // return null for invalid parameter
        if (!valid) return null;

        var output = "N"; // digit 0 = N

        var i = Convert.ToInt32(icao.Substring(1), 16) - 1; // parse icao to int
        if (i < 0) return output;

        var dig1 = i / bucket1_size + 1; // digit 1
        var rem1 = i % bucket1_size;
        output += dig1.ToString();

        if (rem1 < suffix_size) return output + get_suffix(rem1);

        rem1 -= suffix_size; // shift for digit 2
        var dig2 = rem1 / bucket2_size;
        var rem2 = rem1 % bucket2_size;
        output += dig2.ToString();

        if (rem2 < suffix_size) return output + get_suffix(rem2);

        rem2 -= suffix_size; // shift for digit 3
        var dig3 = rem2 / bucket3_size;
        var rem3 = rem2 % bucket3_size;
        output += dig3.ToString();

        if (rem3 < suffix_size) return output + get_suffix(rem3);

        rem3 -= suffix_size; // shift for digit 4
        var dig4 = rem3 / bucket4_size;
        var rem4 = rem3 % bucket4_size;
        output += dig4.ToString();

        if (rem4 == 0) return output;

        // find last character
        return output + allchars[rem4 - 1];
    }

    public string NToIcao(string nnumber)
    {
        // check parameter validity
        var valid = true;
        // verify that tail number has length <=6 and starts with 'N'
        if (!(0 < nnumber.Length && nnumber.Length <= NNUMBER_MAX_SIZE) || nnumber[0] != 'N')
        {
            valid = false;
        }
        else
        {
            // verify the alphabet of the tail number
            foreach (var c in nnumber)
                if (!allchars.Contains(c))
                {
                    valid = false;
                    break;
                }

            // verify that the tail number has a correct format (single suffix at the end of string)
            if (valid && nnumber.Length > 3)
                for (var i = 1; i < nnumber.Length - 2; i++)
                    if (charset.Contains(nnumber[i]))
                    {
                        valid = false;
                        break;
                    }
        }

        if (!valid) return null;

        var prefix = "a";
        var count = 0;

        if (nnumber.Length > 1)
        {
            nnumber = nnumber.Substring(1);
            count += 1;
            for (var i = 0; i < nnumber.Length; i++)
                if (i == NNUMBER_MAX_SIZE - 2) // NNUMBER_MAX_SIZE-2 = 4
                {
                    // last possible char (in allchars)
                    count += allchars.IndexOf(nnumber[i]) + 1;
                }
                else if (charset.Contains(nnumber[i]))
                {
                    // first alphabetical char
                    count += suffix_offset(nnumber.Substring(i)).Value;
                    break; // nothing comes after alphabetical chars
                }
                else
                {
                    // number
                    if (i == 0)
                        count += (int.Parse(nnumber[i].ToString()) - 1) * bucket1_size;
                    else if (i == 1)
                        count += int.Parse(nnumber[i].ToString()) * bucket2_size + suffix_size;
                    else if (i == 2)
                        count += int.Parse(nnumber[i].ToString()) * bucket3_size + suffix_size;
                    else if (i == 3) count += int.Parse(nnumber[i].ToString()) * bucket4_size + suffix_size;
                }
        }

        return CreateIcao(prefix, count);
    }
}