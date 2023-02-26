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

libff::bigint<BIS> EvaluatePolynomial(std::vector<libff::bigint<BIS>> coefficients, libff::bigint<BIS> x) {
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

// Utility functions.

void PrintWords(mp_limb_t words[BIS]) {
  // Converting from vector<uint64_t> to uint256.
  // std::vector<uint64_t> x_words = vk.X.to_words();
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

std::array<uint64_t, WS> wordsToArr(std::vector<uint64_t> words) {
  std::array<uint64_t, WS> arr;
  for (int c = 0; c < WS; c++) {
    arr[c] = words[c];
  }
  return arr;
}

std::vector<uint64_t> arrToWords(std::array<uint64_t, WS> arr) {
  std::vector<uint64_t> words(WS);
  for (int c = 0; c < WS; c++) {
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

bigint<BIS> fromBigInt(BigInt n) {
  bigint<BIS> _n;
  for (int c = 0; c < BIS; c++) {
    _n.data[c] = n.N[c];
  }
  return _n;
}

AltBn128G1 toG1(alt_bn128_G1 p) {
  AltBn128G1 _p;
  _p.X = wordsToArr(p.X.to_words());
  _p.Y = wordsToArr(p.Y.to_words());
  _p.Z = wordsToArr(p.Z.to_words());
  return _p;
}

AltBn128G2 toG2(alt_bn128_G2 p) {
  AltBn128G2 _p;
  _p.X = wordsToArr(p.X.to_words());
  _p.Y = wordsToArr(p.Y.to_words());
  _p.Z = wordsToArr(p.Z.to_words());
  return _p;
}

// Functions to be exported.

void Init() {
  alt_bn128_pp::init_public_params();
  inhibit_profiling_info = true;
}

AltBn128G1 G1() {
  return toG1(alt_bn128_G1::one());
}

AltBn128G2 G2() {
  return toG2(alt_bn128_G2::one());
}

BigInt Order() {
  // Both G1 and G2 have the same order in the case of alt_bn128.
  return toBigInt(alt_bn128_G1::order());
}

BigInt RandomBigInt() {
  libff::bigint<BIS> _n;
  _n.randomize();
  return toBigInt(_n);
}

void _test() {
  Init();
  size_t nParties = 10;
  size_t threshold = 7;
  alt_bn128_G1 G1 = alt_bn128_G1::one();
  alt_bn128_G2 G2 = alt_bn128_G2::one();
  libff::bigint<BIS> r = alt_bn128_G1::order();

  // Generating coefficients.
  std::vector<libff::bigint<BIS>> ZRs(threshold);
  for (size_t c = 0; c < threshold; ++c) {
    libff::bigint<BIS> coef;
    coef.randomize();
    ZRs[c] = coef;
  }

  libff::bigint<BIS> secret = ZRs[threshold-1];

  // Generating secret keys.
  std::vector<bigint<BIS>> sks(nParties);
  for (size_t c = 1; c < nParties+1; c++) {
    sks[c-1] = EvaluatePolynomial(ZRs, c);
  }

  // Generating verification keys.
  alt_bn128_G2 vk = libff::scalar_mul(G2, secret);
  std::vector<alt_bn128_G2> vks(nParties);
  for (size_t c = 0; c < nParties; c++) {
    vks[c] = libff::scalar_mul(G2, sks[c]);
  }

  // Message.
  // TODO From alphanumeric string to G1.
  bigint<BIS> msg = bigint<BIS>();
  msg.randomize();
  alt_bn128_G1 msg_hash = libff::scalar_mul(G1, msg);

  // Generating shares of the message.
  std::vector<alt_bn128_G1> shares(nParties);
  for (size_t c = 0; c < nParties; c++) {
    shares[c] = libff::scalar_mul(msg_hash, sks[c]);
  }

  for (size_t c = 0; c < nParties; c++) {
    bool isEqual =
      alt_bn128_reduced_pairing(msg_hash, vks[c]) ==
      alt_bn128_reduced_pairing(shares[c], G2);
    std::cout << isEqual;
  }
}
