# Aurem
Aurem is a confidential proof-of-stake ("CPoS") mechanism that builds on the work of Aleph Zero's [AlephBFT](https://github.com/aleph-zero-foundation/AlephBFT) and Dusk Network's [PoBB](https://github.com/dusk-network/dusk-blindbid) mechanism. It DAG-based aBFT consensus that ensures three vital parameters in digital currencies.
- Complete privacy of your *bid* (stake) making sure no network participant can see how much currency was staked.
- Instant finality in a round, ensuring fast, reliable, confirmation of a transaction.
- Unpermissioned committees, allowing anyone to become a validator through Aleph's randomness beacon with the head minter selected based on their bid.

Aurem also enforces ring-member selection enforcement at the consensus layer, guaranteeing transaction uniformity to the distribution, selected by the decoy selection algorithm ("DSA"), and provides stronger guarantees that all clients conform to the specification. This greatly mitigates the effects of dusting/flooding attacks for the network, and helps increase privacy for older unspent UTXOs.

# Setup instructions
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
* The current repository **does not represent the final state of Aurem**.
