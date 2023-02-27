#include <vector>
#include <libff/algebra/field_utils/bigint.hpp>

#ifndef AUREM_H
#define AUREM_H

#ifdef __cplusplus
extern "C" {
#endif

#if defined(_WIN32) || defined(__CYGWIN__)
#define EXPORT __declspec(dllexport)
#else
#define EXPORT __attribute__((visibility("default")))
#endif

  // BigInt Size.
  const mp_size_t BIS = 4;
  // Words Size.
  const int WS = 8;

  struct AltBn128G1 {
    std::array<uint64_t, WS> X;
    std::array<uint64_t, WS> Y;
    std::array<uint64_t, WS> Z;
  };

  struct AltBn128G2 {
    std::array<uint64_t, WS> X;
    std::array<uint64_t, WS> Y;
    std::array<uint64_t, WS> Z;
  };

  struct BigInt {
    std::array<uint64_t, WS> N;
  };

  EXPORT void Init();
  EXPORT AltBn128G1 G1();
  EXPORT AltBn128G2 G2();
  EXPORT BigInt Order();
  EXPORT AltBn128G1 RandomFq();
  EXPORT AltBn128G2 RandomFq2();
  EXPORT BigInt RandomCoefficient();
  EXPORT BigInt ModOrder(BigInt n);
  // EXPORT BigInt EvaluatePolynomial(std::vector<BigInt> coefficients, BigInt x);

#ifdef __cplusplus
}
#endif

#endif
