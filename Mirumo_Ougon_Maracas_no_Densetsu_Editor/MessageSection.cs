using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirumo_Ougon_Maracas_no_Densetsu_Editor
{
    public class MessageSection
    {
        public List<MessageBox> MessageBoxes { get; set; } = new();

        public override string ToString()
        {
            return $"0x{MessageBoxes[0].Pointer:X8}";
        }

        public static MessageSection ParseFromStream(FileStream stream, Dictionary<int, Message> messagesMap)
        {
            MessageSection messageSection = new();
            byte[] data = new byte[0x30];
            stream.Read(data, 0, 0x30);
            while (Message.TryParse(data, (int)stream.Position - 0x30, out Message message))
            {
                messagesMap.Add(message.Pointer, message);

                if (stream.Position >= stream.Length - 0x30)
                {
                    break;
                }
                stream.Read(data, 0, 0x30);
            }
            stream.Seek(-0x30, SeekOrigin.Current);

            if (stream.Position >= stream.Length - 0x14)
            {
                return null;
            }

            stream.Read(data, 0, 0x14);
            while (MessageBox.TryParse(data, (int)stream.Position - 0x14, out MessageBox messageBox))
            {
                messageSection.MessageBoxes.Add(messageBox);

                if (stream.Position >= stream.Length - 0x14)
                {
                    break;
                }
                stream.Read(data, 0, 0x14);
            }
            stream.Seek(-0x14, SeekOrigin.Current);
            
            long pos = stream.Position;
            foreach (MessageBox messageBox in messageSection.MessageBoxes)
            {
                foreach (int pointer in messageBox.MessagePointers)
                {
                    if (pointer == 0x00)
                    {
                        continue;
                    }
                    if (!messagesMap.ContainsKey(pointer))
                    { 
                        stream.Seek(pointer, SeekOrigin.Begin);
                        stream.Read(data, 0, 0x30);
                        if (Message.TryParse(data, pointer, out Message message))
                        {
                            messagesMap.Add(pointer, message);
                        }
                        else
                        {
                            throw new FileFormatException($"Could not find message at pointer 0x{pointer:X8}. Requested by message box at {messageBox.Pointer}");
                        }
                    }

                    messageBox.Messages.Add(messagesMap[pointer]);
                }
            }
            stream.Seek(pos, SeekOrigin.Begin);

            return messageSection;
        }
    }

    public class MessageBox
    {
        public const int NUM_LINES = 3;

        public int Pointer { get; set; }
        public List<int> MessagePointers { get; set; } = new();
        public List<Message> Messages { get; set; } = new();
        public short HorizontalSize { get; set; }
        public short VerticalSize { get; set; }

        public override string ToString()
        {
            if (Messages.Count > 0)
            {
                return Messages[0].Value;
            }
            else
            {
                return $"0x{Pointer:X8}";
            }
        }

        public static bool TryParse(byte[] data, int pointer, out MessageBox messageBox)
        {
            messageBox = new();
            messageBox.Pointer = pointer;
            try
            {
                if (data[3] != 0x08)
                {
                    return false;
                }
                for (int i = 0; i < NUM_LINES * 4; i += 4)
                {
                    messageBox.MessagePointers.Add(BitConverter.ToInt32(data.Skip(i).Take(4).ToArray()) & 0x00FFFFFF);
                }
                messageBox.HorizontalSize = BitConverter.ToInt16(data.Skip(NUM_LINES * 4).Take(4).ToArray());
                messageBox.VerticalSize = BitConverter.ToInt16(data.Skip(NUM_LINES * 4 + 2).Take(4).ToArray());

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class Message
    {
        public int Pointer { get; set; }
        public string Value { get; set; } = "";
        public int Length => Value.Length;

        public override string ToString()
        {
            return Value;
        }

        public static bool TryParse(byte[] data, int pointer, out Message message)
        {
            message = new();
            message.Pointer = pointer;
            try
            {
                int length = BitConverter.ToInt32(data.Take(4).ToArray());
                if (length > 0x17)
                {
                    return false;
                }

                for (int i = 4; i < length * 2 + 4; i += 2)
                {
                    message.Value += CharMap[BitConverter.ToUInt16(data.Skip(i).Take(2).ToArray())];
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Dictionary<ushort, string> CharMap = new Dictionary<ushort, string>
        {
            { 0x0000, " " },
            { 0x0001, "あ" },
            { 0x0002, "い" },
            { 0x0003, "う" },
            { 0x0004, "え" },
            { 0x0005, "お" },
            { 0x0006, "か" },
            { 0x0007, "き" },
            { 0x0008, "く" },
            { 0x0009, "け" },
            { 0x000A, "こ" },
            { 0x000B, "さ" },
            { 0x000C, "し" },
            { 0x000D, "す" },
            { 0x000E, "せ" },
            { 0x000F, "そ" },
            { 0x0010, "た" },
            { 0x0011, "ち" },
            { 0x0012, "つ" },
            { 0x0013, "て" },
            { 0x0014, "と" },
            { 0x0015, "な" },
            { 0x0016, "に" },
            { 0x0017, "ぬ" },
            { 0x0018, "ね" },
            { 0x0019, "の" },
            { 0x001A, "は" },
            { 0x001B, "ひ" },
            { 0x001C, "ふ" },
            { 0x001D, "へ" },
            { 0x001E, "ほ" },
            { 0x001F, "ま" },
            { 0x0020, "み" },
            { 0x0021, "む" },
            { 0x0022, "め" },
            { 0x0023, "も" },
            { 0x0024, "や" },
            { 0x0025, "ゆ" },
            { 0x0026, "よ" },
            { 0x0027, "ら" },
            { 0x0028, "り" },
            { 0x0029, "る" },
            { 0x002A, "れ" },
            { 0x002B, "ろ" },
            { 0x002C, "わ" },
            { 0x002D, "を" },
            { 0x002E, "ん" },
            { 0x002F, "っ" },
            { 0x0030, "ゃ" },
            { 0x0031, "ゅ" },
            { 0x0032, "ょ" },
            { 0x0033, "が" },
            { 0x0034, "ぎ" },
            { 0x0035, "ぐ" },
            { 0x0036, "げ" },
            { 0x0037, "ご" },
            { 0x0038, "ざ" },
            { 0x0039, "じ" },
            { 0x003A, "ず" },
            { 0x003B, "ぜ" },
            { 0x003C, "ぞ" },
            { 0x003D, "だ" },
            { 0x003E, "ぢ" },
            { 0x003F, "づ" },
            { 0x0040, "で" },
            { 0x0041, "ど" },
            { 0x0042, "ば" },
            { 0x0043, "び" },
            { 0x0044, "ぶ" },
            { 0x0045, "べ" },
            { 0x0046, "ぼ" },
            { 0x0047, "ぱ" },
            { 0x0048, "ぴ" },
            { 0x0049, "ぷ" },
            { 0x004A, "ぺ" },
            { 0x004B, "ぽ" },
            { 0x004C, "ア" },
            { 0x004D, "イ" },
            { 0x004E, "ウ" },
            { 0x004F, "エ" },
            { 0x0050, "オ" },
            { 0x0051, "カ" },
            { 0x0052, "キ" },
            { 0x0053, "ク" },
            { 0x0054, "ケ" },
            { 0x0055, "コ" },
            { 0x0056, "サ" },
            { 0x0057, "シ" },
            { 0x0058, "ス" },
            { 0x0059, "セ" },
            { 0x005A, "ソ" },
            { 0x005B, "タ" },
            { 0x005C, "チ" },
            { 0x005D, "ツ" },
            { 0x005E, "テ" },
            { 0x005F, "ト" },
            { 0x0060, "ナ" },
            { 0x0061, "ニ" },
            { 0x0062, "ヌ" },
            { 0x0063, "ネ" },
            { 0x0064, "ノ" },
            { 0x0065, "ハ" },
            { 0x0066, "ヒ" },
            { 0x0067, "フ" },
            { 0x0068, "ヘ" },
            { 0x0069, "ホ" },
            { 0x006A, "マ" },
            { 0x006B, "ミ" },
            { 0x006C, "ム" },
            { 0x006D, "メ" },
            { 0x006E, "モ" },
            { 0x006F, "ヤ" },
            { 0x0070, "ユ" },
            { 0x0071, "ヨ" },
            { 0x0072, "ラ" },
            { 0x0073, "リ" },
            { 0x0074, "ル" },
            { 0x0075, "レ" },
            { 0x0076, "ロ" },
            { 0x0077, "ワ" },
            { 0x0078, "ヲ" },
            { 0x0079, "ン" },
            { 0x007A, "ッ" },
            { 0x007B, "ャ" },
            { 0x007C, "ュ" },
            { 0x007D, "ョ" },
            { 0x007E, "ガ" },
            { 0x007F, "ギ" },
            { 0x0080, "グ" },
            { 0x0081, "ゲ" },
            { 0x0082, "ゴ" },
            { 0x0083, "ザ" },
            { 0x0084, "ジ" },
            { 0x0085, "ズ" },
            { 0x0086, "ゼ" },
            { 0x0087, "ゾ" },
            { 0x0088, "ダ" },
            { 0x0089, "ヂ" },
            { 0x008A, "ヅ" },
            { 0x008B, "デ" },
            { 0x008C, "ド" },
            { 0x008D, "バ" },
            { 0x008E, "ビ" },
            { 0x008F, "ブ" },
            { 0x0090, "ベ" },
            { 0x0091, "ボ" },
            { 0x0092, "パ" },
            { 0x0093, "ピ" },
            { 0x0094, "プ" },
            { 0x0095, "ペ" },
            { 0x0096, "ポ" },
            { 0x0097, "ー" },
            { 0x0098, "+" },
            { 0x0099, "!" },
            { 0x009A, "「" },
            { 0x009B, "」" },
            { 0x009C, "、" },
            { 0x009D, "。" },
            { 0x009E, "?" },
            { 0x009F, "…" },
            { 0x00A0, "(" },
            { 0x00A1, "0" },
            { 0x00A2, "1" },
            { 0x00A3, "2" },
            { 0x00A4, "3" },
            { 0x00A5, "4" },
            { 0x00A6, "5" },
            { 0x00A7, "6" },
            { 0x00A8, "7" },
            { 0x00A9, "8" },
            { 0x00AA, "9" },
            { 0x00AB, "a" },
            { 0x00AC, "b" },
            { 0x00AD, "c" },
            { 0x00AE, "d" },
            { 0x00AF, "e" },
            { 0x00B0, "f" },
            { 0x00B1, "g" },
            { 0x00B2, "h" },
            { 0x00B3, "i" },
            { 0x00B4, "j" },
            { 0x00B5, "k" },
            { 0x00B6, "l" },
            { 0x00B7, "m" },
            { 0x00B8, "n" },
            { 0x00B9, "o" },
            { 0x00BA, "p" },
            { 0x00BB, "q" },
            { 0x00BC, "r" },
            { 0x00BD, "s" },
            { 0x00BE, "t" },
            { 0x00BF, "u" },
            { 0x00C0, "v" },
            { 0x00C1, "w" },
            { 0x00C2, "x" },
            { 0x00C3, "y" },
            { 0x00C4, "z" },
            { 0x00C9, "A" },
            { 0x00CA, "B" },
            { 0x00CB, "C" },
            { 0x00CC, "D" },
            { 0x00CD, "E" },
            { 0x00CE, "F" },
            { 0x00CF, "G" },
            { 0x00D0, "H" },
            { 0x00D1, "I" },
            { 0x00D2, "J" },
            { 0x00D3, "K" },
            { 0x00D4, "L" },
            { 0x00D5, "M" },
            { 0x00D6, "N" },
            { 0x00D7, "O" },
            { 0x00D8, "P" },
            { 0x00D9, "Q" },
            { 0x00DA, "R" },
            { 0x00DB, "S" },
            { 0x00DC, "T" },
            { 0x00DD, "U" },
            { 0x00DE, "V" },
            { 0x00DF, "W" },
            { 0x00E0, "X" },
            { 0x00E1, "Y" },
            { 0x00E2, "Z" },
            { 0x00E7, "ぁ" },
            { 0x00E8, "ぃ" },
            { 0x00E9, "ぅ" },
            { 0x00EA, "ぇ" },
            { 0x00EB, "ぉ" },
            { 0x00EC, "ァ" },
            { 0x00ED, "ィ" },
            { 0x00EE, "ゥ" },
            { 0x00EF, "ェ" },
            { 0x00F0, "ォ" },
            { 0x00F1, "…" },
            { 0x00F2, "(" },
            { 0x00F3, ")" },
            { 0x00F4, "/" },
            { 0x00F5, ":" },
            { 0x00F6, "あ" },
            { 0x00F7, "う" },
            { 0x00F8, "お" },
            { 0x00F9, "き" },
            { 0x00FA, "け" },
            { 0x00FB, "さ" },
            { 0x00FC, "す" },
            { 0x00FD, "そ" },
            { 0x00FE, "ち" },
            { 0x00FF, "て" },
            { 0x0200, "使" },
            { 0x0201, "日" },
            { 0x0202, "見" },
            { 0x0203, "団" },
            { 0x0204, "今" },
            { 0x0205, "行" },
            { 0x0206, "来" },
            { 0x0207, "気" },
            { 0x0208, "金" },
            { 0x0209, "黄" },
            { 0x020A, "手" },
            { 0x020B, "人" },
            { 0x020C, "間" },
            { 0x020D, "出" },
            { 0x020E, "聞" },
            { 0x020F, "占" },
            { 0x0210, "世" },
            { 0x0211, "界" },
            { 0x0212, "言" },
            { 0x0213, "様" },
            { 0x0214, "思" },
            { 0x0215, "楽" },
            { 0x0216, "記" },
            { 0x0217, "書" },
            { 0x0218, "南" },
            { 0x0219, "変" },
            { 0x021A, "夜" },
            { 0x021B, "学" },
            { 0x021C, "校" },
            { 0x021D, "時" },
            { 0x021E, "話" },
            { 0x021F, "分" },
            { 0x0220, "入" },
            { 0x0221, "元" },
            { 0x0222, "音" },
            { 0x0223, "前" },
            { 0x0224, "作" },
            { 0x0225, "甘" },
            { 0x0226, "曲" },
            { 0x0227, "本" },
            { 0x0228, "持" },
            { 0x0229, "森" },
            { 0x022A, "力" },
            { 0x022B, "美" },
            { 0x022C, "昼" },
            { 0x022D, "味" },
            { 0x022E, "何" },
            { 0x022F, "中" },
            { 0x0230, "大" },
            { 0x0231, "悪" },
            { 0x0232, "度" },
            { 0x0233, "王" },
            { 0x0234, "当" },
            { 0x0235, "一" },
            { 0x0236, "会" },
            { 0x0237, "知" },
            { 0x0238, "役" },
            { 0x0239, "事" },
            { 0x023A, "取" },
            { 0x023B, "花" },
            { 0x023C, "食" },
            { 0x023D, "子" },
            { 0x023E, "決" },
            { 0x023F, "起" },
            { 0x0240, "身" },
            { 0x0241, "方" },
            { 0x0242, "目" },
            { 0x0243, "私" },
            { 0x0244, "国" },
            { 0x0245, "上" },
            { 0x0246, "強" },
            { 0x0247, "友" },
            { 0x0248, "生" },
            { 0x0249, "通" },
            { 0x024A, "始" },
            { 0x024B, "実" },
            { 0x024C, "星" },
            { 0x024D, "回" },
            { 0x024E, "里" },
            { 0x024F, "考" },
            { 0x0250, "番" },
            { 0x0251, "岩" },
            { 0x0252, "宝" },
            { 0x0253, "族" },
            { 0x0254, "早" },
            { 0x0255, "兵" },
            { 0x0256, "士" },
            { 0x0257, "近" },
            { 0x0258, "名" },
            { 0x0259, "合" },
            { 0x025A, "切" },
            { 0x025B, "新" },
            { 0x025C, "感" },
            { 0x025D, "用" },
            { 0x025E, "心" },
            { 0x025F, "帰" },
            { 0x0260, "水" },
            { 0x0261, "同" },
            { 0x0262, "商" },
            { 0x0263, "声" },
            { 0x0264, "館" },
            { 0x0265, "場" },
            { 0x0266, "待" },
            { 0x0267, "明" },
            { 0x0268, "先" },
            { 0x0269, "少" },
            { 0x026A, "塘" }, // can't tell what this kanji is supposed to be
            { 0x026B, "結" },
            { 0x026C, "意" },
            { 0x026D, "後" },
            { 0x026E, "下" },
            { 0x026F, "次" },
            { 0x0270, "女" },
            { 0x0271, "図" },
            { 0x0272, "落" },
            { 0x0273, "白" },
            { 0x0274, "所" },
            { 0x0275, "動" },
            { 0x0276, "活" },
            { 0x0277, "自" },
            { 0x0278, "体" },
            { 0x0279, "仕" },
            { 0x027A, "終" },
            { 0x027B, "号" },
            { 0x027C, "足" },
            { 0x027D, "面" },
            { 0x027E, "正" },
            { 0x027F, "弱" },
            { 0x0280, "店" },
            { 0x0281, "向" },
            { 0x0282, "口" },
            { 0x0283, "開" },
            { 0x0284, "地" },
            { 0x0285, "兄" },
            { 0x0286, "安" },
            { 0x0287, "守" },
            { 0x0288, "買" },
            { 0x0289, "顔" },
            { 0x028A, "流" },
            { 0x028B, "教" },
            { 0x028C, "返" },
            { 0x028D, "調" },
            { 0x028E, "配" },
            { 0x028F, "笛" },
            { 0x0290, "礼" },
            { 0x0291, "長" },
            { 0x0292, "恋" },
            { 0x0293, "家" },
            { 0x0294, "助" },
            { 0x0295, "売" },
            { 0x0296, "川" },
            { 0x0297, "君" },
            { 0x0298, "物" },
            { 0x0299, "木" },
            { 0x029A, "読" },
            { 0x029B, "直" },
            { 0x029C, "練" },
            { 0x029D, "習" },
            { 0x029E, "送" },
            { 0x029F, "着" },
            { 0x02A0, "発" },
            { 0x02A1, "外" },
            { 0x02A2, "母" },
            { 0x02A3, "想" },
            { 0x02A4, "立" },
            { 0x02A5, "交" },
            { 0x02A6, "負" },
            { 0x02A7, "風" },
            { 0x02A8, "消" },
            { 0x02A9, "神" },
            { 0x02AA, "年" },
            { 0x02AB, "都" },
            { 0x02AC, "内" },
            { 0x02AD, "品" },
            { 0x02AE, "十" },
            { 0x02AF, "予" },
            { 0x02B0, "勝" },
            { 0x02B1, "別" },
            { 0x02B2, "毎" },
            { 0x02B3, "頭" },
            { 0x02B4, "父" },
            { 0x02B5, "土" },
            { 0x02B6, "登" },
            { 0x02B7, "左" },
            { 0x02B8, "男" },
            { 0x02B9, "雲" },
            { 0x02BA, "者" },
            { 0x02BB, "勉" },
            { 0x02BC, "火" },
            { 0x02BD, "島" },
            { 0x02BE, "月" },
            { 0x02BF, "右" },
            { 0x02C0, "~" },
            { 0x02C1, "・" },
        };
    }
}
