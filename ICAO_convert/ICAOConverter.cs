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

/*{ start: 0x700000, end: 0x700FFF, country: "Afghanistan", flag_image: "Afghanistan.png" },
   { start: 0x501000, end: 0x5013FF, country: "Albania", flag_image: "Albania.png" },
   { start: 0x0A0000, end: 0x0A7FFF, country: "Algeria", flag_image: "Algeria.png" },
   { start: 0x090000, end: 0x090FFF, country: "Angola", flag_image: "Angola.png" },
   { start: 0x0CA000, end: 0x0CA3FF, country: "Antigua and Barbuda", flag_image: "Antigua_and_Barbuda.png" },
   { start: 0xE00000, end: 0xE3FFFF, country: "Argentina", flag_image: "Argentina.png" },
   { start: 0x600000, end: 0x6003FF, country: "Armenia", flag_image: "Armenia.png" },
   { start: 0x7C0000, end: 0x7FFFFF, country: "Australia", flag_image: "Australia.png" },
   { start: 0x440000, end: 0x447FFF, country: "Austria", flag_image: "Austria.png" },
   { start: 0x600800, end: 0x600BFF, country: "Azerbaijan", flag_image: "Azerbaijan.png" },
   { start: 0x0A8000, end: 0x0A8FFF, country: "Bahamas", flag_image: "Bahamas.png" },
   { start: 0x894000, end: 0x894FFF, country: "Bahrain", flag_image: "Bahrain.png" },
   { start: 0x702000, end: 0x702FFF, country: "Bangladesh", flag_image: "Bangladesh.png" },
   { start: 0x0AA000, end: 0x0AA3FF, country: "Barbados", flag_image: "Barbados.png" },
   { start: 0x510000, end: 0x5103FF, country: "Belarus", flag_image: "Belarus.png" },
   { start: 0x448000, end: 0x44FFFF, country: "Belgium", flag_image: "Belgium.png" },
   { start: 0x0AB000, end: 0x0AB3FF, country: "Belize", flag_image: "Belize.png" },
   { start: 0x094000, end: 0x0943FF, country: "Benin", flag_image: "Benin.png" },
   { start: 0x680000, end: 0x6803FF, country: "Bhutan", flag_image: "Bhutan.png" },
   { start: 0xE94000, end: 0xE94FFF, country: "Bolivia", flag_image: "Bolivia.png" },
   { start: 0x513000, end: 0x5133FF, country: "Bosnia and Herzegovina", flag_image: "Bosnia.png" },
   { start: 0x030000, end: 0x0303FF, country: "Botswana", flag_image: "Botswana.png" },
   { start: 0xE40000, end: 0xE7FFFF, country: "Brazil", flag_image: "Brazil.png" },
   { start: 0x895000, end: 0x8953FF, country: "Brunei Darussalam", flag_image: "Brunei.png" },
   { start: 0x450000, end: 0x457FFF, country: "Bulgaria", flag_image: "Bulgaria.png" },
   { start: 0x09C000, end: 0x09CFFF, country: "Burkina Faso", flag_image: "Burkina_Faso.png" },
   { start: 0x032000, end: 0x032FFF, country: "Burundi", flag_image: "Burundi.png" },
   { start: 0x70E000, end: 0x70EFFF, country: "Cambodia", flag_image: "Cambodia.png" },
   { start: 0x034000, end: 0x034FFF, country: "Cameroon", flag_image: "Cameroon.png" },
   { start: 0xC00000, end: 0xC3FFFF, country: "Canada", flag_image: "Canada.png" },
   { start: 0x096000, end: 0x0963FF, country: "Cape Verde", flag_image: "Cape_Verde.png" },
   { start: 0x06C000, end: 0x06CFFF, country: "Central African Republic", flag_image: "Central_African_Republic.png" },
   { start: 0x084000, end: 0x084FFF, country: "Chad", flag_image: "Chad.png" },
   { start: 0xE80000, end: 0xE80FFF, country: "Chile", flag_image: "Chile.png" },
   { start: 0x780000, end: 0x7BFFFF, country: "China", flag_image: "China.png" },
   { start: 0x0AC000, end: 0x0ACFFF, country: "Colombia", flag_image: "Colombia.png" },
   { start: 0x035000, end: 0x0353FF, country: "Comoros", flag_image: "Comoros.png" },
   { start: 0x036000, end: 0x036FFF, country: "Congo", flag_image: "Republic_of_the_Congo.png" }, // probably?
   { start: 0x901000, end: 0x9013FF, country: "Cook Islands", flag_image: "Cook_Islands.png" },
   { start: 0x0AE000, end: 0x0AEFFF, country: "Costa Rica", flag_image: "Costa_Rica.png" },
   { start: 0x038000, end: 0x038FFF, country: "Cote d'Ivoire", flag_image: "Cote_d_Ivoire.png" },
   { start: 0x501C00, end: 0x501FFF, country: "Croatia", flag_image: "Croatia.png" },
   { start: 0x0B0000, end: 0x0B0FFF, country: "Cuba", flag_image: "Cuba.png" },
   { start: 0x4C8000, end: 0x4C83FF, country: "Cyprus", flag_image: "Cyprus.png" },
   { start: 0x498000, end: 0x49FFFF, country: "Czech Republic", flag_image: "Czech_Republic.png" },
   { start: 0x720000, end: 0x727FFF, country: "Democratic People's Republic of Korea", flag_image: "North_Korea.png" },
   { start: 0x08C000, end: 0x08CFFF, country: "Democratic Republic of the Congo", flag_image: "Democratic_Republic_of_the_Congo.png" },
   { start: 0x458000, end: 0x45FFFF, country: "Denmark", flag_image: "Denmark.png" },
   { start: 0x098000, end: 0x0983FF, country: "Djibouti", flag_image: "Djibouti.png" },
   { start: 0x0C4000, end: 0x0C4FFF, country: "Dominican Republic", flag_image: "Dominican_Republic.png" },
   { start: 0xE84000, end: 0xE84FFF, country: "Ecuador", flag_image: "Ecuador.png" },
   { start: 0x010000, end: 0x017FFF, country: "Egypt", flag_image: "Egypt.png" },
   { start: 0x0B2000, end: 0x0B2FFF, country: "El Salvador", flag_image: "El_Salvador.png" },
   { start: 0x042000, end: 0x042FFF, country: "Equatorial Guinea", flag_image: "Equatorial_Guinea.png" },
   { start: 0x202000, end: 0x2023FF, country: "Eritrea", flag_image: "Eritrea.png" },
   { start: 0x511000, end: 0x5113FF, country: "Estonia", flag_image: "Estonia.png" },
   { start: 0x040000, end: 0x040FFF, country: "Ethiopia", flag_image: "Ethiopia.png" },
   { start: 0xC88000, end: 0xC88FFF, country: "Fiji", flag_image: "Fiji.png" },
   { start: 0x460000, end: 0x467FFF, country: "Finland", flag_image: "Finland.png" },
   { start: 0x380000, end: 0x3BFFFF, country: "France", flag_image: "France.png" },
   { start: 0x03E000, end: 0x03EFFF, country: "Gabon", flag_image: "Gabon.png" },
   { start: 0x09A000, end: 0x09AFFF, country: "Gambia", flag_image: "Gambia.png" },
   { start: 0x514000, end: 0x5143FF, country: "Georgia", flag_image: "Georgia.png" },
   { start: 0x3C0000, end: 0x3FFFFF, country: "Germany", flag_image: "Germany.png" },
   { start: 0x044000, end: 0x044FFF, country: "Ghana", flag_image: "Ghana.png" },
   { start: 0x468000, end: 0x46FFFF, country: "Greece", flag_image: "Greece.png" },
   { start: 0x0CC000, end: 0x0CC3FF, country: "Grenada", flag_image: "Grenada.png" },
   { start: 0x0B4000, end: 0x0B4FFF, country: "Guatemala", flag_image: "Guatemala.png" },
   { start: 0x046000, end: 0x046FFF, country: "Guinea", flag_image: "Guinea.png" },
   { start: 0x048000, end: 0x0483FF, country: "Guinea-Bissau", flag_image: "Guinea_Bissau.png" },
   { start: 0x0B6000, end: 0x0B6FFF, country: "Guyana", flag_image: "Guyana.png" },
   { start: 0x0B8000, end: 0x0B8FFF, country: "Haiti", flag_image: "Haiti.png" },
   { start: 0x0BA000, end: 0x0BAFFF, country: "Honduras", flag_image: "Honduras.png" },
   { start: 0x470000, end: 0x477FFF, country: "Hungary", flag_image: "Hungary.png" },
   { start: 0x4CC000, end: 0x4CCFFF, country: "Iceland", flag_image: "Iceland.png" },
   { start: 0x800000, end: 0x83FFFF, country: "India", flag_image: "India.png" },
   { start: 0x8A0000, end: 0x8A7FFF, country: "Indonesia", flag_image: "Indonesia.png" },
   { start: 0x730000, end: 0x737FFF, country: "Iran, Islamic Republic of", flag_image: "Iran.png" },
   { start: 0x728000, end: 0x72FFFF, country: "Iraq", flag_image: "Iraq.png" },
   { start: 0x4CA000, end: 0x4CAFFF, country: "Ireland", flag_image: "Ireland.png" },
   { start: 0x738000, end: 0x73FFFF, country: "Israel", flag_image: "Israel.png" },
   { start: 0x300000, end: 0x33FFFF, country: "Italy", flag_image: "Italy.png" },
   { start: 0x0BE000, end: 0x0BEFFF, country: "Jamaica", flag_image: "Jamaica.png" },
   { start: 0x840000, end: 0x87FFFF, country: "Japan", flag_image: "Japan.png" },
   { start: 0x740000, end: 0x747FFF, country: "Jordan", flag_image: "Jordan.png" },
   { start: 0x683000, end: 0x6833FF, country: "Kazakhstan", flag_image: "Kazakhstan.png" },
   { start: 0x04C000, end: 0x04CFFF, country: "Kenya", flag_image: "Kenya.png" },
   { start: 0xC8E000, end: 0xC8E3FF, country: "Kiribati", flag_image: "Kiribati.png" },
   { start: 0x706000, end: 0x706FFF, country: "Kuwait", flag_image: "Kuwait.png" },
   { start: 0x601000, end: 0x6013FF, country: "Kyrgyzstan", flag_image: "Kyrgyzstan.png" },
   { start: 0x708000, end: 0x708FFF, country: "Lao People's Democratic Republic", flag_image: "Laos.png" },
   { start: 0x502C00, end: 0x502FFF, country: "Latvia", flag_image: "Latvia.png" },
   { start: 0x748000, end: 0x74FFFF, country: "Lebanon", flag_image: "Lebanon.png" },
   { start: 0x04A000, end: 0x04A3FF, country: "Lesotho", flag_image: "Lesotho.png" },
   { start: 0x050000, end: 0x050FFF, country: "Liberia", flag_image: "Liberia.png" },
   { start: 0x018000, end: 0x01FFFF, country: "Libyan Arab Jamahiriya", flag_image: "Libya.png" },
   { start: 0x503C00, end: 0x503FFF, country: "Lithuania", flag_image: "Lithuania.png" },
   { start: 0x4D0000, end: 0x4D03FF, country: "Luxembourg", flag_image: "Luxembourg.png" },
   { start: 0x054000, end: 0x054FFF, country: "Madagascar", flag_image: "Madagascar.png" },
   { start: 0x058000, end: 0x058FFF, country: "Malawi", flag_image: "Malawi.png" },
   { start: 0x750000, end: 0x757FFF, country: "Malaysia", flag_image: "Malaysia.png" },
   { start: 0x05A000, end: 0x05A3FF, country: "Maldives", flag_image: "Maldives.png" },
   { start: 0x05C000, end: 0x05CFFF, country: "Mali", flag_image: "Mali.png" },
   { start: 0x4D2000, end: 0x4D23FF, country: "Malta", flag_image: "Malta.png" },
   { start: 0x900000, end: 0x9003FF, country: "Marshall Islands", flag_image: "Marshall_Islands.png" },
   { start: 0x05E000, end: 0x05E3FF, country: "Mauritania", flag_image: "Mauritania.png" },
   { start: 0x060000, end: 0x0603FF, country: "Mauritius", flag_image: "Mauritius.png" },
   { start: 0x0D0000, end: 0x0D7FFF, country: "Mexico", flag_image: "Mexico.png" },
   { start: 0x681000, end: 0x6813FF, country: "Micronesia, Federated States of", flag_image: "Micronesia.png" },
   { start: 0x4D4000, end: 0x4D43FF, country: "Monaco", flag_image: "Monaco.png" },
   { start: 0x682000, end: 0x6823FF, country: "Mongolia", flag_image: "Mongolia.png" },
   { start: 0x516000, end: 0x5163FF, country: "Montenegro", flag_image: "Montenegro.png" },
   { start: 0x020000, end: 0x027FFF, country: "Morocco", flag_image: "Morocco.png" },
   { start: 0x006000, end: 0x006FFF, country: "Mozambique", flag_image: "Mozambique.png" },
   { start: 0x704000, end: 0x704FFF, country: "Myanmar", flag_image: "Myanmar.png" },
   { start: 0x201000, end: 0x2013FF, country: "Namibia", flag_image: "Namibia.png" },
   { start: 0xC8A000, end: 0xC8A3FF, country: "Nauru", flag_image: "Nauru.png" },
   { start: 0x70A000, end: 0x70AFFF, country: "Nepal", flag_image: "Nepal.png" },
   { start: 0x480000, end: 0x487FFF, country: "Netherlands, Kingdom of the", flag_image: "Netherlands.png" },
   { start: 0xC80000, end: 0xC87FFF, country: "New Zealand", flag_image: "New_Zealand.png" },
   { start: 0x0C0000, end: 0x0C0FFF, country: "Nicaragua", flag_image: "Nicaragua.png" },
   { start: 0x062000, end: 0x062FFF, country: "Niger", flag_image: "Niger.png" },
   { start: 0x064000, end: 0x064FFF, country: "Nigeria", flag_image: "Nigeria.png" },
   { start: 0x478000, end: 0x47FFFF, country: "Norway", flag_image: "Norway.png" },
   { start: 0x70C000, end: 0x70C3FF, country: "Oman", flag_image: "Oman.png" },
   { start: 0x760000, end: 0x767FFF, country: "Pakistan", flag_image: "Pakistan.png" },
   { start: 0x684000, end: 0x6843FF, country: "Palau", flag_image: "Palau.png" },
   { start: 0x0C2000, end: 0x0C2FFF, country: "Panama", flag_image: "Panama.png" },
   { start: 0x898000, end: 0x898FFF, country: "Papua New Guinea", flag_image: "Papua_New_Guinea.png" },
   { start: 0xE88000, end: 0xE88FFF, country: "Paraguay", flag_image: "Paraguay.png" },
   { start: 0xE8C000, end: 0xE8CFFF, country: "Peru", flag_image: "Peru.png" },
   { start: 0x758000, end: 0x75FFFF, country: "Philippines", flag_image: "Philippines.png" },
   { start: 0x488000, end: 0x48FFFF, country: "Poland", flag_image: "Poland.png" },
   { start: 0x490000, end: 0x497FFF, country: "Portugal", flag_image: "Portugal.png" },
   { start: 0x06A000, end: 0x06A3FF, country: "Qatar", flag_image: "Qatar.png" },
   { start: 0x718000, end: 0x71FFFF, country: "Republic of Korea", flag_image: "South_Korea.png" },
   { start: 0x504C00, end: 0x504FFF, country: "Republic of Moldova", flag_image: "Moldova.png" },
   { start: 0x4A0000, end: 0x4A7FFF, country: "Romania", flag_image: "Romania.png" },
   { start: 0x100000, end: 0x1FFFFF, country: "Russian Federation", flag_image: "Russian_Federation.png" },
   { start: 0x06E000, end: 0x06EFFF, country: "Rwanda", flag_image: "Rwanda.png" },
   { start: 0xC8C000, end: 0xC8C3FF, country: "Saint Lucia", flag_image: "Saint_Lucia.png" },
   { start: 0x0BC000, end: 0x0BC3FF, country: "Saint Vincent and the Grenadines", flag_image: "Saint_Vincent_and_the_Grenadines.png" },
   { start: 0x902000, end: 0x9023FF, country: "Samoa", flag_image: "Samoa.png" },
   { start: 0x500000, end: 0x5003FF, country: "San Marino", flag_image: "San_Marino.png" },
   { start: 0x09E000, end: 0x09E3FF, country: "Sao Tome and Principe", flag_image: "Sao_Tome_and_Principe.png" },
   { start: 0x710000, end: 0x717FFF, country: "Saudi Arabia", flag_image: "Saudi_Arabia.png" },
   { start: 0x070000, end: 0x070FFF, country: "Senegal", flag_image: "Senegal.png" },
   { start: 0x4C0000, end: 0x4C7FFF, country: "Serbia", flag_image: "Serbia.png" },
   { start: 0x074000, end: 0x0743FF, country: "Seychelles", flag_image: "Seychelles.png" },
   { start: 0x076000, end: 0x0763FF, country: "Sierra Leone", flag_image: "Sierra_Leone.png" },
   { start: 0x768000, end: 0x76FFFF, country: "Singapore", flag_image: "Singapore.png" },
   { start: 0x505C00, end: 0x505FFF, country: "Slovakia", flag_image: "Slovakia.png" },
   { start: 0x506C00, end: 0x506FFF, country: "Slovenia", flag_image: "Slovenia.png" },
   { start: 0x897000, end: 0x8973FF, country: "Solomon Islands", flag_image: "Soloman_Islands.png" }, // flag typo?
   { start: 0x078000, end: 0x078FFF, country: "Somalia", flag_image: "Somalia.png" },
   { start: 0x008000, end: 0x00FFFF, country: "South Africa", flag_image: "South_Africa.png" },
   { start: 0x340000, end: 0x37FFFF, country: "Spain", flag_image: "Spain.png" },
   { start: 0x770000, end: 0x777FFF, country: "Sri Lanka", flag_image: "Sri_Lanka.png" },
   { start: 0x07C000, end: 0x07CFFF, country: "Sudan", flag_image: "Sudan.png" },
   { start: 0x0C8000, end: 0x0C8FFF, country: "Suriname", flag_image: "Suriname.png" },
   { start: 0x07A000, end: 0x07A3FF, country: "Swaziland", flag_image: "Swaziland.png" },
   { start: 0x4A8000, end: 0x4AFFFF, country: "Sweden", flag_image: "Sweden.png" },
   { start: 0x4B0000, end: 0x4B7FFF, country: "Switzerland", flag_image: "Switzerland.png" },
   { start: 0x778000, end: 0x77FFFF, country: "Syrian Arab Republic", flag_image: "Syria.png" },
   { start: 0x515000, end: 0x5153FF, country: "Tajikistan", flag_image: "Tajikistan.png" },
   { start: 0x880000, end: 0x887FFF, country: "Thailand", flag_image: "Thailand.png" },
   { start: 0x512000, end: 0x5123FF, country: "The former Yugoslav Republic of Macedonia", flag_image: "Macedonia.png" },
   { start: 0x088000, end: 0x088FFF, country: "Togo", flag_image: "Togo.png" },
   { start: 0xC8D000, end: 0xC8D3FF, country: "Tonga", flag_image: "Tonga.png" },
   { start: 0x0C6000, end: 0x0C6FFF, country: "Trinidad and Tobago", flag_image: "Trinidad_and_Tobago.png" },
   { start: 0x028000, end: 0x02FFFF, country: "Tunisia", flag_image: "Tunisia.png" },
   { start: 0x4B8000, end: 0x4BFFFF, country: "Turkey", flag_image: "Turkey.png" },
   { start: 0x601800, end: 0x601BFF, country: "Turkmenistan", flag_image: "Turkmenistan.png" },
   { start: 0x068000, end: 0x068FFF, country: "Uganda", flag_image: "Uganda.png" },
   { start: 0x508000, end: 0x50FFFF, country: "Ukraine", flag_image: "Ukraine.png" },
   { start: 0x896000, end: 0x896FFF, country: "United Arab Emirates", flag_image: "UAE.png" },
   { start: 0x400000, end: 0x43FFFF, country: "United Kingdom", flag_image: "United_Kingdom.png" },
   { start: 0x080000, end: 0x080FFF, country: "United Republic of Tanzania", flag_image: "Tanzania.png" },
   { start: 0xA00000, end: 0xAFFFFF, country: "United States", flag_image: "United_States_of_America.png" },
   { start: 0xE90000, end: 0xE90FFF, country: "Uruguay", flag_image: "Uruguay.png" },
   { start: 0x507C00, end: 0x507FFF, country: "Uzbekistan", flag_image: "Uzbekistan.png" },
   { start: 0xC90000, end: 0xC903FF, country: "Vanuatu", flag_image: "Vanuatu.png" },
   { start: 0x0D8000, end: 0x0DFFFF, country: "Venezuela", flag_image: "Venezuela.png" },
   { start: 0x888000, end: 0x88FFFF, country: "Viet Nam", flag_image: "Vietnam.png" },
   { start: 0x890000, end: 0x890FFF, country: "Yemen", flag_image: "Yemen.png" },
   { start: 0x08A000, end: 0x08AFFF, country: "Zambia", flag_image: "Zambia.png" },
   { start: 0x004000, end: 0x0043FF, country: "Zimbabwe", flag_image: "Zimbabwe.png" },

*/