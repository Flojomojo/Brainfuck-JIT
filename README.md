# Brainfuck JIT in C#

## Features
- Syntax error handling
- Runs any bf "program"
- Easy to understand code
- Interpreter in C#
- JIT compiles to nasm
- Tested only on linux (might work on windows with minor modifications)

## How to run
- Have nasm installed
- Have dotnet sdk version >= 8.0 installed
- Clone it and cd into the folder
- Run via "dotnet run {bf_program.bf} {binary_output_filename}"

## References
- https://en.wikipedia.org/wiki/Brainfuck
- https://gist.github.com/roachhd/dce54bec8ba55fb17d3a
