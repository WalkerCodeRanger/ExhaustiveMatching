# Examples

This project contains examples of using the `ExhaustiveMatching.Analyzer`. It uses a `.editorconfig` file to configure what would normally be errors from the analyzer as suggestions because many of them are intended.

## Default Namespace

The default namespace of this project is `Examples` rather than `ExhaustiveMatching.Examples` as one would expect. This is because placing them in the `ExhaustiveMatching` namespace causes using statements of that namespace to be unnecessary. However, the examples should show what the code would really look like in a project using the analyzer. In such projects, the `using ExhaustiveMatching;` directive would be necessary. In some places, omitted code is indicated by ellipses. In the source code, these are marked by `/* … */`. In the read me file, these are replaced with `…`

## ReadMe

Files in this folder and namespace are used as examples in the project `README.md` at root of the repository. When code is placed into the read me, lines are omitted to focus only on the important details. The lines included in the read me are only those inside of the `snippet` regions. Generally, a new line is included between snippets from the file and indenting is changed to remove extra indenting levels.
