using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace stack;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<NoteData>))]
[JsonSerializable(typeof(NoteData))]
internal partial class NoteDataJsonContext : JsonSerializerContext
{
}
