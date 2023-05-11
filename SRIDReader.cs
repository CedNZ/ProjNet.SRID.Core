using System.Reflection;
using System.Text;
using ProjNet.CoordinateSystems;

namespace ProjNet.SRID.Core
{
    public class SRIDReader
    {
        public struct WktString
        {
            public int WktId;

            public string Wkt;
        }

        private static readonly Lazy<CoordinateSystemFactory> CoordinateSystemFactory = new Lazy<CoordinateSystemFactory>(() => new CoordinateSystemFactory());

        public static IEnumerable<WktString> GetSrids(string? filename = null)
        {
            Stream? stream = string.IsNullOrWhiteSpace(filename) 
                ? Assembly.GetExecutingAssembly().GetManifestResourceStream("ProjNet.SRID.Core.SRID.csv") 
                : File.OpenRead(filename);

            if (stream is null)
            {
                yield return new WktString();
            }

            using StreamReader sr = new StreamReader(stream!, Encoding.UTF8);
            while (!sr.EndOfStream)
            {
                string? line = sr.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    int split = line.IndexOf(';');
                    if (split > -1)
                    {
                        yield return new WktString
                        {
                            WktId = int.Parse(line[..split]),
                            Wkt = line[(split + 1)..]
                        };
                    }
                }
            }
        }

        public static CoordinateSystem? GetCSbyID(int id, string? file = null)
        {
            IEnumerable<WktString> srids = GetSrids(file);
            foreach (WktString item in srids)
            {
                if (item.WktId == id)
                {
                    return CoordinateSystemFactory.Value.CreateFromWkt(item.Wkt);
                }
            }
            return null;
        }
    }

}