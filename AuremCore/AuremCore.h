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

  struct AltBn128G1 {
    std::array<uint64_t, 4> X;
    std::array<uint64_t, 4> Y;
    std::array<uint64_t, 4> Z;
  };

  struct AltBn128G2 {
    std::array<uint64_t, 8> X;
    std::array<uint64_t, 8> Y;
    std::array<uint64_t, 8> Z;
  };

  struct BigInt {
    std::array<uint64_t, 4> N;
  };

  EXPORT void Init();
  EXPORT AltBn128G1 G1();
  EXPORT AltBn128G2 G2();
  EXPORT BigInt Order();
  EXPORT bool EqualG1(AltBn128G1 p1, AltBn128G1 p2);
  EXPORT bool EqualG2(AltBn128G2 p1, AltBn128G2 p2);
  EXPORT AltBn128G1 ScalarMulG1(BigInt n);
  EXPORT AltBn128G2 ScalarMulG2(BigInt n);
  EXPORT AltBn128G1 ScalarPointMulG1(AltBn128G1 point, BigInt n);
  EXPORT AltBn128G2 ScalarPointMulG2(AltBn128G2 point, BigInt n);
  EXPORT bool PairsEqual(AltBn128G1 p1G1, AltBn128G2 p1G2, AltBn128G1 p2G1, AltBn128G2 p2G2);
  EXPORT BigInt RandomCoefficient();
  EXPORT BigInt ModOrder(BigInt n);
  EXPORT AltBn128G1 AddG1(AltBn128G1 p1, AltBn128G1 p2);
  EXPORT AltBn128G2 AddG2(AltBn128G2 p1, AltBn128G2 p2);
  EXPORT void PrintAffineG1(AltBn128G1 p);

#ifdef __cplusplus
}
#endif

#endif
