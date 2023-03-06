#include "AuremCore.h"

#include <gmp.h>
#include <libff/algebra/curves/alt_bn128/alt_bn128_fields.hpp>
#include <libff/algebra/curves/alt_bn128/alt_bn128_g1.hpp>
#include <libff/algebra/curves/alt_bn128/alt_bn128_g2.hpp>
#include <libff/algebra/curves/alt_bn128/alt_bn128_pairing.hpp>
#include <libff/algebra/curves/alt_bn128/alt_bn128_pp.hpp>
#include <libff/algebra/curves/curve_utils.hpp>
#include <libff/algebra/field_utils/bigint.hpp>
#include <libff/algebra/field_utils/bigint.tcc>
#include <libff/algebra/fields/prime_base/fp.hpp>
#include <libff/common/serialization.hpp>
#include <boost/multiprecision/cpp_int.hpp>

using namespace boost::multiprecision;
using namespace libff;

// Utility functions.

void PrintWords(mp_limb_t words[BIS]) {
  uint256_t x = 0;
  for (int i = 7; i >= 0; i--) {
    x <<= 64;
    x |= words[i];
  }
  std::cout << "PrintWords:\n" << x << "\n";
}

void PrintWords(std::vector<uint64_t> words) {
  uint256_t x = 0;
  for (int i = 7; i >= 0; i--) {
    x <<= 64;
    x |= words[i];
  }
  std::cout << "PrintWords:\n" << x << "\n";
}

std::array<uint64_t, 4> wordsToArr4(std::vector<uint64_t> words) {
  std::array<uint64_t, 4> arr;
  for (int c = 0; c < 4; c++) {
    arr[c] = words[c];
  }
  return arr;
}

std::array<uint64_t, 8> wordsToArr8(std::vector<uint64_t> words) {
  std::array<uint64_t, 8> arr;
  for (int c = 0; c < 8; c++) {
    arr[c] = words[c];
  }
  return arr;
}

std::vector<uint64_t> arrToWords4(std::array<uint64_t, 4> arr) {
  std::vector<uint64_t> words(4);
  for (int c = 0; c < 4; c++) {
    words[c] = arr[c];
  }
  return words;
}

std::vector<uint64_t> arrToWords8(std::array<uint64_t, 8> arr) {
  std::vector<uint64_t> words(8);
  for (int c = 0; c < 8; c++) {
    words[c] = arr[c];
  }
  return words;
}

BigInt toBigInt(bigint<BIS> n) {
  BigInt _n;
  for (int c = 0; c < BIS; c++) {
    _n.N[c] = n.data[c];
  }
  return _n;
}

BigInt toBigInt(mpz_t n) {
  return toBigInt(bigint<BIS>(n));
}

bigint<BIS> fromBigInt(BigInt n) {
  bigint<BIS> _n;
  for (int c = 0; c < BIS; c++) {
    _n.data[c] = n.N[c];
  }
  return _n;
}

AltBn128G1 toG1(alt_bn128_G1 p) {
  AltBn128G1 _p;
  _p.X = wordsToArr4(p.X.to_words());
  _p.Y = wordsToArr4(p.Y.to_words());
  _p.Z = wordsToArr4(p.Z.to_words());
  return _p;
}

AltBn128G2 toG2(alt_bn128_G2 p) {
  AltBn128G2 _p;
  _p.X = wordsToArr8(p.X.to_words());
  _p.Y = wordsToArr8(p.Y.to_words());
  _p.Z = wordsToArr8(p.Z.to_words());
  return _p;
}

alt_bn128_G1 fromG1(AltBn128G1 p) {
  alt_bn128_G1 _p;
  _p.X.from_words(arrToWords4(p.X));
  _p.Y.from_words(arrToWords4(p.Y));
  _p.Z.from_words(arrToWords4(p.Z));
  return _p;
}

alt_bn128_G2 fromG2(AltBn128G2 p) {
  alt_bn128_G2 _p;
  _p.X.from_words(arrToWords8(p.X));
  _p.Y.from_words(arrToWords8(p.Y));
  _p.Z.from_words(arrToWords8(p.Z));
  return _p;
}

AltBn128G1 AddG1(AltBn128G1 p1, AltBn128G1 p2) {
  return toG1(fromG1(p1).add(fromG1(p2)));
}

void PrintAffineG1(AltBn128G1 p) {
  alt_bn128_G1 _p = fromG1(p);
  _p.to_affine_coordinates();
  _p.print_coordinates();
}

AltBn128G2 AddG2(AltBn128G2 p1, AltBn128G2 p2) {
  return toG2(fromG2(p1).add(fromG2(p2)));
}

bigint<BIS> EvaluatePolynomial(std::vector<libff::bigint<BIS>> coefficients, libff::bigint<BIS> x) {
  bigint<BIS> result;
  mpz_t mpz_result;
  mpz_t mpz_x;
  mpz_t mpz_order;

  // Initializing.
  mpz_init_set_ui(mpz_result, 0);
  mpz_init_set_ui(mpz_x, 0);
  mpz_init_set_ui(mpz_order, 0);

  alt_bn128_G1::order().to_mpz(mpz_order);
  x.to_mpz(mpz_x);

  mpz_t mpz_coef;
  mpz_t tmp;
  mpz_init_set_ui(mpz_coef, 0);
  mpz_init_set_ui(tmp, 0);

  for (size_t c = 0; c < coefficients.size(); ++c) {
    coefficients[c].to_mpz(mpz_coef);

    mpz_set(tmp, mpz_result);

    // result = result + coefficients[c] + mul(result, x);
    mpz_mul(mpz_result, mpz_result, mpz_x);
    mpz_add(mpz_result, mpz_result, mpz_coef);
    mpz_add(mpz_result, mpz_result, tmp);
  }
  // mpz_clear(mpz_coef);
  // mpz_clear(tmp);

  mpz_mod(mpz_result, mpz_result, mpz_order);

  return bigint<BIS>(mpz_result);
}

// Functions to be exported.

alt_bn128_G1 _G1;
alt_bn128_G2 _G2;
bigint<BIS> _Order;
mpz_t _mpz_order;

void Init() {
  alt_bn128_pp::init_public_params();
  inhibit_profiling_info = true;
  _G1 = alt_bn128_G1::one();
  _G2 = alt_bn128_G2::one();
  _Order = alt_bn128_G1::order();
  mpz_init(_mpz_order);
  _Order.to_mpz(_mpz_order);
}

AltBn128G1 G1() {
  return toG1(_G1);
}

AltBn128G2 G2() {
  return toG2(_G2);
}

BigInt Order() {
  // Both G1 and G2 have the same order in the case of alt_bn128.
  return toBigInt(_Order);
}

BigInt ModOrder(BigInt n) {
  mpz_t _n;
  mpz_init_set_ui(_n, 0);
  fromBigInt(n).to_mpz(_n);
  mpz_mod(_n, _n, _mpz_order);
  return toBigInt(_n);
}

BigInt RandomCoefficient() {
  mpz_t _coeff;
  mpz_init_set_ui(_coeff, 0);
  mpz_random(_coeff, 4);
  mpz_mod(_coeff, _coeff, _mpz_order);
  return toBigInt(_coeff);
}

bool EqualG1(AltBn128G1 p1, AltBn128G1 p2) {
  return fromG1(p1) == fromG1(p2);
}

bool EqualG2(AltBn128G2 p1, AltBn128G2 p2) {
  return fromG2(p1) == fromG2(p2);
}

AltBn128G1 ScalarMulG1(BigInt n) {
  return toG1(libff::scalar_mul(_G1, fromBigInt(n)));
}

AltBn128G2 ScalarMulG2(BigInt n) {
  return toG2(libff::scalar_mul(_G2, fromBigInt(n)));
}

AltBn128G1 ScalarPointMulG1(AltBn128G1 point, BigInt n) {
  return toG1(libff::scalar_mul(fromG1(point), fromBigInt(n)));
}

AltBn128G2 ScalarPointMulG2(AltBn128G2 point, BigInt n) {
  return toG2(libff::scalar_mul(fromG2(point), fromBigInt(n)));
}

BigInt EvaluatePolynomial(std::vector<BigInt> coefficients, BigInt x) {
  bigint<BIS> _x = fromBigInt(x);
  int csize = coefficients.size();
  std::vector<bigint<BIS>> _coefficients = std::vector<bigint<BIS>>(csize);
  for (int c = 0; c < csize; c++) {
    _coefficients[c] = fromBigInt(coefficients[c]);
  }
  return toBigInt(EvaluatePolynomial(_coefficients, _x));
}

bool PairsEqual(AltBn128G1 p1G1, AltBn128G2 p1G2,
                AltBn128G1 p2G1, AltBn128G2 p2G2) {
  return
    alt_bn128_reduced_pairing(fromG1(p1G1), fromG2(p1G2)) ==
    alt_bn128_reduced_pairing(fromG1(p2G1), fromG2(p2G2));
}
