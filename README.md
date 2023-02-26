# Aurem

These instructions assume:
- `libff` header files are located at `/usr/local/include/libff`
- `libff` is located at `/usr/local/lib`
- .NET 7.0 is installed. It should also work with .NET 6.0, though.
- Linux.

Instructions:

- Install [libff](https://github.com/scipr-lab/libff)
- Set desired parameters in `config.json`
  - `numNodes`: how many nodes to simulate in the network
  - `fixedRounds`: `-1` to run forever, `fixedRounds > 0` to run a finite number of rounds
  - `graphsDirectory`: the name of the directory (inside `$HOME`) to store the generated chDAGs
- `dotnet build -p:StartupObject=Aurem.Program`
- To compile `AuremCore`:

``` shell
g++ -shared -fPIC -I/usr/local/include/libff -L/usr/local/lib -Wl,-rpath=/usr/local/lib AuremCore/AuremCore.cpp -lff -lgmp -o bin/Debug/net7.0/AuremCore.so
```

- `dotnet run`
- `./graphs.sh compile` to create PNGs from the generated Graphviz dotfiles
- `./graphs.sh clean` to delete the graphs directory and its contents

A recommendation is to run

``` shell
./graphs.sh clean && dotnet run && ./graphs.sh compile
```

to remove previous graphs, and generate and compile new ones.

To run tests:

``` shell
dotnet test -p:StartupObject=Aurem.Program
```

# Notes

* For now, any experimental or debugging features is contained in `Program.cs`
  (entry-point). For instance, the rest of the source code files never access
  any of the configuration parameters stored in `config.json`.
* `graphs.sh` only works for *nix systems.
