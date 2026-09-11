using System.Collections.Generic;
using System.Text;

namespace QL
{
    public enum CharMask
    {
        Letter = 1,
        Number = 2,
        UnderLine = 4,
        Special = 8,
        LetterNumber = Letter | Number,
        LetterNumberUnderLine = Letter | Number | UnderLine,
        All = Letter | Number | UnderLine | Special,
    }

    public static class CharacterVerifyHelper
    {
        private static byte[] getTemplateVerifyArrayByMask(CharMask mask)
        {
            byte[] r = new byte[128];
            if ((mask & CharMask.Letter) != 0)
            {
                //A-Z
                for (int i = 65; i <= 90; i++)
                    r[i] = 1;
                //a-z
                for (int i = 97; i <= 122; i++)
                    r[i] = 1;
            }
            if ((mask & CharMask.Number) != 0)
            {
                //0-9
                for (int i = 48; i <= 57; i++)
                    r[i] = 1;
            }
            if ((mask & CharMask.UnderLine) != 0)
            {
                //_
                r[95] = 1;
            }
            if ((mask & CharMask.Special) != 0)
            {
                r[33] = 1;  //!
                r[34] = 1;  //"
                r[35] = 1;  //#
                r[36] = 1;  //$
                r[37] = 1;  //%
                r[38] = 1;  //&
                r[39] = 1;  //'
                r[40] = 1;  //(
                r[41] = 1;  //)
                r[42] = 1;  //*
                r[43] = 1;  //+
                r[44] = 1;  //,
                r[45] = 1;  //-
                r[46] = 1;  //.
                r[47] = 1;  ///
                r[58] = 1;  //:
                r[59] = 1;  //;
                r[60] = 1;  //<
                r[61] = 1;  //=
                r[62] = 1;  //>
                r[63] = 1;  //?
                r[64] = 1;  //@
                r[91] = 1;  //[
                r[92] = 1;  //\
                r[93] = 1;  //]
                r[94] = 1;  //^
                r[96] = 1;  //`
                r[123] = 1; //{
                r[124] = 1; //|
                r[125] = 1; //}
                r[126] = 1; //~
            }
            return r;
        }

        private static Dictionary<int, byte[]> maskMap_ = new Dictionary<int, byte[]>();

        public static bool CheckValid(string contentToCheck, CharMask mask)
        {
            return CheckValidWithLength(contentToCheck, mask, 0, 0);
        }
        
        //allow length range: [minLength, maxLength]
        public static bool CheckValidWithLength(string contentToCheck, CharMask mask, int minLength, int maxLength)
        {
            bool success = true;
            int key = (int)mask;

            lock (maskMap_)
            {
                byte[] arr = null;
                if (!maskMap_.TryGetValue(key, out arr))
                {
                    arr = getTemplateVerifyArrayByMask(mask);
                    maskMap_[key] = arr;
                }

                var contentBytes = Encoding.UTF8.GetBytes(contentToCheck);
                if (maxLength <= 0 || (contentBytes.Length >= minLength && contentBytes.Length <= maxLength))
                {
                    foreach (var b in contentBytes)
                    {
                        if (b >= arr.Length || arr[b] == 0)
                        {
                            success = false;
                            break;
                        }
                    }
                }
                else
                {
                    success = false;
                }
            }

            return success;
        }
    }
}