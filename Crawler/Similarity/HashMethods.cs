using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    public static class HashMethods
    {
        public static HashMethod Convert(HashAlgorithm alg)
        {
            return alg.ComputeHash;
        }

        public static HashMethod Sequence(HashMethod method1, params HashMethod[] methods)
        {
            return x =>
            {
                x = method1(x);

                foreach (var m in methods)
                    x = m(x);

                return x;
            };
        }

        private static IEnumerable<HashMethod> getAll()
        {
            yield return MD5;
            yield return RIPEMD160;
            yield return SHA1;
            yield return SHA256;
            yield return SHA384;
            yield return SHA512;
        }

        public static IEnumerable<HashMethod> MixAll(int level = 1)
        {
            var list = getAll();

            for (int i = 2; i <= level; i++)
                list = mixAll(list);

            return list;
        }

        public static IEnumerable<HashMethod> MixCount(int count)
        {
            var list = getAll();

            while (list.Count() < count)
                list = mixAll(list);

            return list.Take(count);
        }

        private static IEnumerable<HashMethod> mixAll(IEnumerable<HashMethod> inputMethods)
        {
            var methods = inputMethods.ToArray();

            foreach (var m in methods)
                yield return m;

            for (int i = 0; i < methods.Length; i++)
                foreach (var m in getAll())
                    yield return Sequence(methods[i], m);
        }

        public static readonly HashMethod MD5 = Convert(System.Security.Cryptography.MD5.Create());
        public static readonly HashMethod RIPEMD160 = Convert(System.Security.Cryptography.RIPEMD160.Create());
        public static readonly HashMethod SHA1 = Convert(System.Security.Cryptography.SHA1.Create());
        public static readonly HashMethod SHA256 = Convert(System.Security.Cryptography.SHA256.Create());
        public static readonly HashMethod SHA384 = Convert(System.Security.Cryptography.SHA384.Create());
        public static readonly HashMethod SHA512 = Convert(System.Security.Cryptography.SHA512.Create());
    }
}
