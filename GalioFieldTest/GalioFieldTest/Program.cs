using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalioFieldTest
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            var A = BitWise32.Parse(2, "10101");
            var B = BitWise32.Parse(2, "10011");
            Console.WriteLine("A = " + A + " irredcable = " + A.isIrreducable);
            Console.WriteLine("B = " + B + " irredcable = " + B.isIrreducable);
            var M = A.Multiply(B);
            Console.WriteLine("A x B = " + M + " irredcable = " + M.isIrreducable);
            var bmod = BitWise32.Parse(2, "100");
            var R = M.Mod(bmod);
            var Q = M.Divide(bmod);
            Console.WriteLine("M % (" + bmod + ") = " + R + " irredcable = " + R.isIrreducable);
            Console.WriteLine("M / (" + bmod + ") = " + Q + " irredcable = " + Q.isIrreducable);
            Console.ReadLine();
            */

            var pp = BitWise32.Parse(2, "101001");
            Console.WriteLine(pp.ToString() + " : " + pp.isIrreducable);

            GalioField GF = new GalioField(3, 8);
            //foreach (var c in GF.Column)
            //    Console.WriteLine("C = " + c + " irredcable = " + c.isIrreducable);
            Console.WriteLine("Irr = " + GF.IrreducablePoly);
            
            string gf = GF.ToString();
            System.IO.File.WriteAllText("GF.txt", gf);
            Console.WriteLine(gf);

            /*
            List<string> polys = new List<string>() 
            { 
                "01100011",
                "01111100",
                "01110111",
                "01111001",
                "11110010",
                "01101001",
                "01101111",
                "11000101"
            };
            foreach (var p in polys)
            {
                var ori = BitWise32.Parse(2, p);
                var inv = GF.FindInverse(ori);
                Console.WriteLine("Inverse of " + ori + " is " + inv);
            }*/

            Console.ReadLine();
        }
    }

    public class GalioField
    {
        /// <summary>
        /// base of galio field
        /// </summary>
        public uint P { get; private set; }

        /// <summary>
        /// order of galio field
        /// </summary>
        public uint N { get; private set; }

        /// <summary>
        /// Column of the galio field
        /// </summary>
        public BitWise32[] Column;

        /// <summary>
        /// Data of the galio field
        /// </summary>
        public BitWise32[,] Data;

        /// <summary>
        /// Irreducable Polynomial in case of over
        /// </summary>
        public BitWise32 IrreducablePoly;

        /// <summary>
        /// Constructor of Galio Field
        /// </summary>
        /// <param name="P"></param>
        /// <param name="N"></param>
        public GalioField(uint P, uint N, uint SkipIrreducable = 0)
        {
            this.P = P;
            this.N = N;
            int Size = (int)Math.Pow(P, N) - 1;
            Column = new BitWise32[Size];
            Column[0] = new BitWise32(P).Accend();
            for (int i = 1; i < Column.Length; i++)
                Column[i] = Column[i - 1].Accend();
            //find irreducable
            IrreducablePoly = Column[Size - 1].Accend();
            while (true)
            {
                IrreducablePoly = IrreducablePoly.Accend();
                if(IrreducablePoly.isIrreducable)
                {
                    if(SkipIrreducable > 0)
                        SkipIrreducable--;
                    else
                        break;
                }
            }
            Console.WriteLine("Find IRR = " + IrreducablePoly);
            //generate data
            Data = new BitWise32[Size, Size];
            //
            Parallel.For(0, Size, (x)=>
                {
                    Console.WriteLine(x + "/" + Size);
                    for (int y = 0; y < Size; y++)
                        Data[x, y] = Column[x].Multiply(Column[y]).Mod(IrreducablePoly);
                });
            /*
            for (int x = 0; x < Size; x++)

            */
        }

        /// <summary>
        /// Find inverse of a bit wise
        /// </summary>
        /// <param name="poly">polymonial</param>
        /// <returns></returns>
        public BitWise32 FindInverse(BitWise32 poly)
        {
            int col = -1;
            //find this poly first
            for (int i =0;i<Column.Length;i++)
            {
                BitWise32 sub = poly.Sub(Column[i].Clone(), 0);
                if(sub.HighestNoneZeroBit < 0)
                {
                    col = i;
                    break;
                }
            }
            //prevent none found
            if (col < 0)
                return null;
            //found 1
            for (int i = 0; i < Column.Length; i++)
                if (Data[col, i].HighestNoneZeroBit == 0)
                    return Column[i].Clone();
            return null;
        }

        /// <summary>
        /// Convert galio field to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("GF(" + P + "^" + N + ")\t" + IrreducablePoly.ToString()+"\n");
            for (int x = 0; x < Column.Length; x++)
            {
                for (int y = 0; y < Column.Length; y++)
                {
                    sb.Append(Data[x, y] + "\t");
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }
    }

    public class BitWise32
    {
        /// <summary>
        /// Value of bitWise
        /// </summary>
        public int[] Value { get; private set; }

        /// <summary>
        /// Mod value of bitwise
        /// </summary>
        public uint Modular { get; private set; }

        /// <summary>
        /// highest none zero bit of bitwise
        /// </summary>
        public int HighestNoneZeroBit
        {
            get
            {
                int h = -1;
                for (int i = 0; i < 32; i++)
                    if (Value[i] > 0)
                        h = i;
                return h;
            }
        }

        /// <summary>
        /// Check if a bitwise is irreducable
        /// </summary>
        public bool isIrreducable
        {
            get
            {
                if (Modular == 0)
                    return false;
                //get highest bit
                int highest = this.HighestNoneZeroBit;
                //generate a bitwise to divide
                BitWise32 div = new BitWise32(Modular);
                //set all div bits to highest
                for (int i = 0; i < highest; i++)
                    div.Value[i] = (int)Modular - 1;
                //loop through all case
                while (div.HighestNoneZeroBit > 0)
                {
                    //get mod reminder
                    BitWise32 r = this.Mod(div);
                    //reminder = 0 for reducable
                    if (r.HighestNoneZeroBit < 0)
                        return false;
                    //reduce the divider
                    div = div.Reduce();
                }
                return true;
            }
        }

        /// <summary>
        /// Constructor of BitWise 32
        /// </summary>
        public BitWise32(uint Modular = 0)
        {
            this.Value = new int[32];
            this.Modular = Modular;
        }

        /// <summary>
        /// Accend a bitwise from end
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BitWise32 Accend()
        {
            BitWise32 div = this.Clone();
            //reduce the divider
            for (int i = 0; i < 32; i++)
            {
                //reduce without carry
                if (div.Value[i] < Modular - 1)
                {
                    div.Value[i]++;
                    break;
                }
                else
                    div.Value[i] = 0;
            }
            return div;
        }

        /// <summary>
        /// reduce a bitwise from end
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BitWise32 Reduce()
        {
            BitWise32 div = this.Clone();
            //reduce the divider
            for (int i = 0; i < 32; i++)
            {
                //reduce without carry
                if (div.Value[i] > 0)
                {
                    div.Value[i]--;
                    break;
                }
                else
                    div.Value[i] = (int)Modular - 1;
            }
            return div;
        }

        /// <summary>
        /// create a binary bit wise from value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BitWise32 Clone()
        {
            BitWise32 bit = new BitWise32(Modular);
            //copy array
            for (int i = 0; i < 32; i++)
                bit.Value[i] = this.Value[i];
            return bit;
        }

        /// <summary>
        /// Parse a bitwise value from string
        /// </summary>
        /// <param name="P"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static BitWise32 Parse(uint P, string str)
        {
            BitWise32 value = new BitWise32(P);
            int index = 0;
            for (int i = str.Length - 1; i >= 0; i--)
            {
                int v = Convert.ToInt32(str.ElementAt(i));
                v %= (int)P;
                value.Value[index++] = v;
            }
            return value;
        }

        /// <summary>
        /// Mod 2 bitwise object
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public BitWise32 Mod(BitWise32 b)
        {
            BitWise32 output = this.Clone();
            while (true)
            {
                int HBase = output.HighestNoneZeroBit;
                int HMod = b.HighestNoneZeroBit;
                //if highest modular had highest bit further than base
                if (HMod > HBase)
                    return output;
                //if highest modular had highest bit same as base, compare the first bit
                //else if (HMod == HBase && b.Value[HMod] > output.Value[HBase])
                //    return output;
                else
                {
                    int offset = HBase - HMod;
                    //output = output.Add(b, offset);
                    output = output.Sub(b, offset);
                }
            }
        }

        /// <summary>
        /// Divide 2 bitwise object
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public BitWise32 Divide(BitWise32 b)
        {
            BitWise32 baseNum = this.Clone();
            BitWise32 output = new BitWise32(Modular);
            while (true)
            {
                int HBase = baseNum.HighestNoneZeroBit;
                int HMod = b.HighestNoneZeroBit;
                //if highest modular had highest bit further than base
                if (HMod > HBase)
                    return output;
                //if highest modular had highest bit same as base, compare the first bit
                //else if (HMod == HBase && b.Value[HMod] > baseNum.Value[HBase])
                //    return output;
                else
                {
                    int offset = HBase - HMod;
                    //baseNum = baseNum.Add(b, offset);
                    baseNum = baseNum.Sub(b, offset);
                    output.Value[offset]++;
                }
            }
        }

        /// <summary>
        /// Multiply 2 bitwise object
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public BitWise32 Multiply(BitWise32 b)
        {
            int[] Output = new int[32];
            for (int i = 0; i < 32; i++)
                for (int j = 0; j < 32; j++)
                    if((i + j) < 32)
                        Output[i + j] += Value[i] * b.Value[j];
            if(Modular != 0)
                for (int i = 0; i < 32; i++)
                    Output[i] %= (int)Modular;
            BitWise32 bit = new BitWise32(this.Modular);
            bit.Value = Output;
            return bit;
        }

        /// <summary>
        /// Add 2 bitwise object
        /// </summary>
        /// <param name="b"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public BitWise32 Add(BitWise32 b, int offset)
        {
            int[] Output = new int[32];
            for (int i = offset; i < 32; i++)
                Output[i] += Value[i] + b.Value[i - offset];
            for (int i = 0; i < offset; i++)
                Output[i] = Value[i];
            if (Modular != 0)
                for (int i = 0; i < 32; i++)
                    Output[i] %= (int)Modular;
            BitWise32 bit = new BitWise32(this.Modular);
            bit.Value = Output;
            return bit;
        }

        /// <summary>
        /// Subtract 2 bitwise object
        /// </summary>
        /// <param name="b"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public BitWise32 Sub(BitWise32 b, int offset)
        {
            int[] Output = new int[32];
            for (int i = offset; i < 32; i++)
                Output[i] += Value[i] - b.Value[i - offset];
            for (int i = 0; i < offset; i++)
                Output[i] = Value[i];
            if (Modular != 0)
                for (int i = 0; i < 32; i++)
                    Output[i] = (Output[i] + (int)Modular) % (int)Modular;
            BitWise32 bit = new BitWise32(this.Modular);
            bit.Value = Output;
            return bit;
        }

        /// <summary>
        /// Add 2 bitwise object
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public BitWise32 Add(BitWise32 b)
        {
            return Add(b, 0);
        }

        /// <summary>
        /// Convert bitwise to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            for(int i=31;i>=0;i--)
            {
                if (this.Value[i] > 1 || (i == 0 && this.Value[i] > 0))
                {
                    if (!first)
                        sb.Append(" + ");
                    sb.Append(this.Value[i]);
                    if (i > 0)
                    {
                        sb.Append("x^");
                        sb.Append(i);
                    }
                    first = false;                   
                }
                else if (this.Value[i] == 1)
                {
                    if (!first)
                        sb.Append(" + ");
                    if (i == 1)
                    {
                        sb.Append("x");
                    }
                    else
                    {
                        sb.Append("x^");
                        sb.Append(i);
                    }
                    first = false;
                }
            }
            return sb.ToString().Length > 0 ? sb.ToString() : "0";
        }
    }
}
