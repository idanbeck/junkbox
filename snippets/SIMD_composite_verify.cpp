#include <stdio.h>
#include <pmmintrin.h>
#include <immintrin.h>
#include <array>
#include <cstring>

__attribute__((always_inline)) __m128 _mm_add3_ps_composite( const __m128& a, const __m128& b, const __m128& c ) {
	return _mm_add_ps( _mm_add_ps( a, b ), c );
}

int main() {
	printf("hi\n");

	__m128 a = _mm_set1_ps(5.0f);
	__m128 b = _mm_set1_ps(7.0f);
	__m128 c = _mm_set1_ps(9.0f);

	auto val = _mm_add3_ps_composite(a, b, c);

	printf("return val: {%f, %f, %f, %f}\n", val[0], val[1], val[2], val[3]);

	return 0;
}
