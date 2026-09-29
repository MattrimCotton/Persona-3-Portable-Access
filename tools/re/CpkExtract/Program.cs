// Extrait les fichiers d'une archive CPK du jeu (bibliothèque CriFsV2Lib de Sewer56).
// Usage : dotnet run --project tools/re/CpkExtract -- <archive.cpk> <dossier sortie> [filtre]
// Le filtre est une liste d'extensions ou de fragments de chemin séparés par des virgules
// (ex. ".bmd,.bf"), sans filtre : liste seulement le contenu.
using CriFsV2Lib;

if (args.Length < 2) { Console.WriteLine("usage: CpkExtract <cpk> <outdir> [filtre]"); return 1; }
string cpk = args[0], outDir = args[1];
string[] filters = args.Length > 2 ? args[2].Split(',', StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();

using var stream = new FileStream(cpk, FileMode.Open, FileAccess.Read);
using var reader = CriFsLib.Instance.CreateCpkReader(stream, true);
var files = reader.GetFiles();
Console.WriteLine($"{files.Length} fichiers dans {cpk}");
int n = 0;
foreach (var f in files)
{
    string path = string.IsNullOrEmpty(f.Directory) ? f.FileName : f.Directory + "/" + f.FileName;
    if (filters.Length == 0) { Console.WriteLine($"{path}\t{f.ExtractSize}"); continue; }
    if (!filters.Any(x => path.Contains(x, StringComparison.OrdinalIgnoreCase))) continue;
    using var data = reader.ExtractFile(f);
    string dst = Path.Combine(outDir, path);
    Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
    File.WriteAllBytes(dst, data.Span.ToArray());
    n++;
}
if (filters.Length > 0) Console.WriteLine($"{n} fichiers extraits dans {outDir}");
return 0;
