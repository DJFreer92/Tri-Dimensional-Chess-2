using UnityEngine;
using UnityEditor;
using System.IO;

[DisallowMultipleComponent]
public sealed class FileManager : MonoSingleton<FileManager> {
    ///<summary>
    ///Opens a file selection panel to allow the user to select an FEN or PGN file
    ///then reads the file and returns the contents
    ///</summary>
    ///<returns>The contents of a selected FEN or PGN file, or null if no file was selected</returns>
    public string GetFENPGNFromFile() {
        return ExtractFENPGN(OpenFilePanelForFENPGN());
    }

    ///<summary>
    ///Opens a file selection panel to allow the user to select and FEN or PGN file
    ///then returns the file path
    ///</summary>
    ///<returns>The file path of the selected file</returns>
    private string OpenFilePanelForFENPGN() {
        return EditorUtility.OpenFilePanel("Select FEN/PGN file (.fen/.pgn)", "", "fen,pgn");
    }

    ///<summary>
    ///Returns the contents of the file at the filepath
    ///</summary>
    ///<param name="path">The filepath of the file to be read</param>
    ///<returns>The contents of the file at the filepath</returns>
    private string ExtractFENPGN(string path) {
        if (string.IsNullOrEmpty(path)) return null;
        using var file = new StreamReader(path);
        return file.ReadToEnd();
    }
}
