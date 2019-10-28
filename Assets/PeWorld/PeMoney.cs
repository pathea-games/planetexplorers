using System.IO;

namespace Pathea
{
    public class Money
    {
        const int MoneyItemProtoId = 229;
        public static bool Digital = true;

        int mCur;
        Pathea.PackageCmpt mPkg;

        public Money()
        {
        }

        public Money(Pathea.PackageCmpt pkg)
        {
            mPkg = pkg;
        }

        public int current
        {
            get
            {
                if (Digital)
                {
                    return digitalCur;
                }
                else
                {
                    return itemCur;
                }
            }

            set
            {
                if (Digital)
                {
                    digitalCur = value;
                }
                else
                {
                    itemCur = value;
                }
            }
        }

        int digitalCur
        {
            get
            {
                return mCur;
            }
            set
            {
                mCur = value;
            }
        }

        int itemCur
        {
            get
            {
                return mPkg.GetItemCount(MoneyItemProtoId);
            }

            set
            {
                mPkg.Set(MoneyItemProtoId, value);
            }
        }

        public void SetCur(int n)
        {
            mCur = n;
        }

        public byte[] Export()
        {
            return PETools.Serialize.Export((w) =>
            {
                w.Write((int)mCur);
            });
        }

        public void Import(byte[] data)
        {
            PETools.Serialize.Import(data, (r) =>
            {
                mCur = r.ReadInt32();
            });
        }
    }

}