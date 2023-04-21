# Changelog

All notable changes to Aurem will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.2]

### Added
 
- Implemented a way to generate a permutation of units in a round, which ensures
  that the choosing of a head unit&mdash;which is used for creating a linear
  ordering of the units in the consensus process&mdash;is non-deterministic.

## [0.0.1] - 2019-04-13

### Added

- Added this CHANGELOG.md for our community to read how Aurem progresses.
- Aurem prototype. This prototype is capable of simulating a number of nodes
  asynchronously broadcasting units of data to other nodes. Nodes are able to
  achieve consensus, which means that every node shares the same structure and
  ordering of the units in their DAG (directed acyclic graph).

[unreleased]: https://github.com/Discreet/aurem/compare/v0.0.1...HEAD
[0.0.1]: https://github.com/Discreet/aurem/releases/tag/v0.0.1
