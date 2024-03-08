// Copyright 2012 the V8 project authors. All rights reserved.
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
//       copyright notice, this list of conditions and the following
//       disclaimer in the documentation and/or other materials provided
//       with the distribution.
//     * Neither the name of Google Inc. nor the names of its
//       contributors may be used to endorse or promote products derived
//       from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using int16_t = System.Int16;
using int32_t = System.Int32;
using uint32_t = System.UInt32;
using uint64_t = System.UInt64;
using Utility = Chomp.Util.Utility;

[SkipLocalsInit]
public static unsafe class double_conversion {
    public enum FastDtoaMode : byte {
        // Computes the shortest representation of the given input. The returned
        // result will be the most accurate number of this length. Longer
        // representations might be more accurate.
        FAST_DTOA_SHORTEST,
        // Same as FAST_DTOA_SHORTEST but for single-precision floats.
        FAST_DTOA_SHORTEST_SINGLE,
        // Computes a representation where the precision (number of digits) is
        // given as input. The precision is independent of the decimal point.
        FAST_DTOA_PRECISION,
    }

    // The minimal and maximal target exponent define the range of w's binary
    // exponent, where 'w' is the result of multiplying the input by a cached power
    // of ten.
    //
    // A different range might be chosen on a different platform, to optimize digit
    // generation, but a smaller range requires more powers of ten to be cached.
    public const int32_t kMinimalTargetExponent = -60;
    public const int32_t kMaximalTargetExponent = -32;

    // FastDtoa will produce at most kFastDtoaMaximalLength digits. This does not
    // include the terminating '\0' character.
    public const int32_t kFastDtoaMaximalLength = 17;
    // Same for single-precision numbers.
    public const int32_t kFastDtoaMaximalSingleLength = 9;

    // Returns the biggest power of ten that is less than or equal to the given
    // number. We furthermore receive the maximum number of bits 'number' has.
    //
    // Returns power == 10^(exponent_plus_one-1) such that
    //    power <= number < power * 10.
    // If number_bits == 0 then 0^(0-1) is returned.
    // The number of bits must be <= 32.
    // Precondition: number < (1 << (number_bits + 1)).
    //
    // Inspired by the method for finding an integer log base 10 from here:
    // http://graphics.stanford.edu/~seander/bithacks.html#IntegerLog10
    public static ReadOnlySpan<uint32_t> kSmallPowersOfTen =>
        new uint32_t[] { 0, 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000 };

    // static R static_cast<R>(object P) => (R)P;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Conditional("DEBUG")]
    public static void DOUBLE_CONVERSION_ASSERT(bool condition, [CallerArgumentExpression("condition")] string condition_message = "<unspecified>") {
        if (!condition) throw new AssertionError(condition_message);
    }


    // Adjusts the last digit of the generated number, and screens out generated
    // solutions that may be inaccurate. A solution may be inaccurate if it is
    // outside the safe interval, or if we cannot prove that it is closer to the
    // input than a neighboring representation of the same length.
    //
    // Input: * buffer containing the digits of too_high / 10^kappa
    //        * the buffer's length
    //        * distance_too_high_w == (too_high - w).f() * unit
    //        * unsafe_interval == (too_high - too_low).f() * unit
    //        * rest = (too_high - buffer * 10^kappa).f() * unit
    //        * ten_kappa = 10^kappa * unit
    //        * unit = the common multiplier
    // Output: returns true if the buffer is guaranteed to contain the closest
    //    representable number to the input.
    //  Modifies the generated digits in the buffer to approach (round towards) w.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool RoundWeed(Span<char> buffer,
                                  int32_t    length,
                                  uint64_t   distance_too_high_w,
                                  uint64_t   unsafe_interval,
                                  uint64_t   rest,
                                  uint64_t   ten_kappa,
                                  uint64_t   unit) {
        uint64_t small_distance = distance_too_high_w - unit;
        uint64_t big_distance   = distance_too_high_w + unit;
        // Let w_low  = too_high - big_distance, and
        //     w_high = too_high - small_distance.
        // Note: w_low < w < w_high
        //
        // The real w (* unit) must lie somewhere inside the interval
        // ]w_low; w_high[ (often written as "(w_low; w_high)")

        // Basically the buffer currently contains a number in the unsafe interval
        // ]too_low; too_high[ with too_low < w < too_high
        //
        //  too_high - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        //                     ^v 1 unit            ^      ^                 ^      ^
        //  boundary_high ---------------------     .      .                 .      .
        //                     ^v 1 unit            .      .                 .      .
        //   - - - - - - - - - - - - - - - - - - -  +  - - + - - - - - -     .      .
        //                                          .      .         ^       .      .
        //                                          .  big_distance  .       .      .
        //                                          .      .         .       .    rest
        //                              small_distance     .         .       .      .
        //                                          v      .         .       .      .
        //  w_high - - - - - - - - - - - - - - - - - -     .         .       .      .
        //                     ^v 1 unit                   .         .       .      .
        //  w ----------------------------------------     .         .       .      .
        //                     ^v 1 unit                   v         .       .      .
        //  w_low  - - - - - - - - - - - - - - - - - - - - -         .       .      .
        //                                                           .       .      v
        //  buffer --------------------------------------------------+-------+--------
        //                                                           .       .
        //                                                  safe_interval    .
        //                                                           v       .
        //   - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -     .
        //                     ^v 1 unit                                     .
        //  boundary_low -------------------------                     unsafe_interval
        //                     ^v 1 unit                                     v
        //  too_low  - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        //
        //
        // Note that the value of buffer could lie anywhere inside the range too_low
        // to too_high.
        //
        // boundary_low, boundary_high and w are approximations of the real boundaries
        // and v (the input number). They are guaranteed to be precise up to one unit.
        // In fact the error is guaranteed to be strictly less than one unit.
        //
        // Anything that lies outside the unsafe interval is guaranteed not to round
        // to v when read again.
        // Anything that lies inside the safe interval is guaranteed to round to v
        // when read again.
        // If the number inside the buffer lies inside the unsafe interval but not
        // inside the safe interval then we simply do not know and bail out (returning
        // false).
        //
        // Similarly we have to take into account the imprecision of 'w' when finding
        // the closest representation of 'w'. If we have two potential
        // representations, and one is closer to both w_low and w_high, then we know
        // it is closer to the actual value v.
        //
        // By generating the digits of too_high we got the largest (closest to
        // too_high) buffer that is still in the unsafe interval. In the case where
        // w_high < buffer < too_high we try to decrement the buffer.
        // This way the buffer approaches (rounds towards) w.
        // There are 3 conditions that stop the decrementation process:
        //   1) the buffer is already below w_high
        //   2) decrementing the buffer would make it leave the unsafe interval
        //   3) decrementing the buffer would yield a number below w_high and farther
        //      away than the current number. In other words:
        //              (buffer{-1} < w_high) && w_high - buffer{-1} > buffer - w_high
        // Instead of using the buffer directly we use its distance to too_high.
        // Conceptually rest ~= too_high - buffer
        // We need to do the following tests in this order to avoid over- and
        // underflows.
        DOUBLE_CONVERSION_ASSERT(rest <= unsafe_interval);

        while (rest < small_distance &&               // Negated condition 1
               unsafe_interval - rest >= ten_kappa && // Negated condition 2
               rest + ten_kappa >= small_distance &&  // buffer{-1} > w_high
               small_distance - rest < rest + ten_kappa - small_distance) {
            Utility.GetReference(buffer, length - 1)--;
            rest += ten_kappa;
        }

        // We have approached w+ as much as possible. We now test if approaching w-
        // would require changing the buffer. If yes, then we have two possible
        // representations close to w, but we cannot decide which one is closer.
        return (rest >= big_distance ||
                unsafe_interval - rest < ten_kappa ||
                rest + ten_kappa < big_distance ||
                big_distance - rest > rest + ten_kappa - big_distance) &&
               // Weeding test.
               //   The safe interval is [too_low + 2 ulp; too_high - 2 ulp]
               //   Since too_low = too_high - unsafe_interval this is equivalent to
               //      [too_high - unsafe_interval + 4 ulp; too_high - 2 ulp]
               //   Conceptually we have: rest ~= too_high - buffer
               2 * unit <= rest && rest <= unsafe_interval - 4 * unit;
    }


    // Rounds the buffer upwards if the result is closer to v by possibly adding
    // 1 to the buffer. If the precision of the calculation is not sufficient to
    // round correctly, return false.
    // The rounding might shift the whole buffer in which case the kappa is
    // adjusted. For example "99", kappa = 3 might become "10", kappa = 4.
    //
    // If 2*rest > ten_kappa then the buffer needs to be round up.
    // rest can have an error of +/- 1 unit. This function accounts for the
    // imprecision and returns false, if the rounding direction cannot be
    // unambiguously determined.
    //
    // Precondition: rest < ten_kappa.
    public static bool RoundWeedCounted(Span<char> buffer,
                                         int32_t    length,
                                         uint64_t   rest,
                                         uint64_t   ten_kappa,
                                         uint64_t   unit,
                                         int32_t*   kappa) {
        DOUBLE_CONVERSION_ASSERT(rest < ten_kappa);
        // The following tests are done in a specific order to avoid overflows. They
        // will work correctly with any uint64 values of rest < ten_kappa and unit.
        //
        // If the unit is too big, then we don't know which way to round. For example
        // a unit of 50 means that the real number lies within rest +/- 50. If
        // 10^kappa == 40 then there is no way to tell which way to round.
        if (unit >= ten_kappa) return false;

        // Even if unit is just half the size of 10^kappa we are already completely
        // lost. (And after the previous test we know that the expression will not
        // over/underflow.)
        if (ten_kappa - unit <= unit) return false;

        // If 2 * (rest + unit) <= 10^kappa we can safely round down.
        if (ten_kappa - rest > rest && ten_kappa - 2 * rest >= 2 * unit) return true;
        // If 2 * (rest - unit) >= 10^kappa, then we can safely round up.
        if (rest <= unit || ten_kappa - (rest - unit) > rest - unit) return false;
        // Increment the last digit recursively until we find a non '9' digit.
        buffer[length - 1]++;

        for (int32_t i = length - 1; i > 0; --i) {
            if (buffer[i] != '0' + 10) break;

            buffer[i] = '0';
            buffer[i - 1]++;
        }

        // If the first digit is now '0'+ 10 we had a buffer with all '9's. With the
        // exception of the first digit all digits are now '0'. Simply switch the
        // first digit to '1' and adjust the kappa. Example: "99" becomes "10" and
        // the power (the kappa) is increased.
        if (buffer[0] != '0' + 10) return true;
        buffer[0] =  '1';
        *kappa    += 1;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void BiggestPowerTen(uint32_t     number,
                                        int32_t      number_bits,
                                        out uint32_t power,
                                        out int32_t  exponent_plus_one) {
        DOUBLE_CONVERSION_ASSERT(number < 1u << number_bits + 1);
        // 1233/4096 is approximately 1/log2(10).
        exponent_plus_one = (number_bits + 1) * 1233 >> 12;
        // We increment to skip over the first entry in the kPowersOf10 table.
        // Note: kPowersOf10[i] == 10^(i-1).
        //exponent_plus_one_guess++;
        // We don't have any guarantees that 2^number_bits <= number.
        //if (number < kSmallPowersOfTen[exponent_plus_one_guess]) {
        //    exponent_plus_one_guess--;
        //}
        ref uint32_t powerOfTens = ref Utility.GetReference(kSmallPowersOfTen);
        exponent_plus_one += Unsafe.BitCast<bool, byte>(number >= Unsafe.Add(ref powerOfTens, exponent_plus_one + 1));
        power             =  Unsafe.Add(ref powerOfTens, exponent_plus_one);
    }

    // Generates the digits of input number w.
    // w is a floating-point number (DiyFp), consisting of a significand and an
    // exponent. Its exponent is bounded by kMinimalTargetExponent and
    // kMaximalTargetExponent.
    //       Hence -60 <= w.e() <= -32.
    //
    // Returns false if it fails, in which case the generated digits in the buffer
    // should not be used.
    // Preconditions:
    //  * low, w and high are correct up to 1 ulp (unit in the last place). That
    //    is, their error must be less than a unit of their last digits.
    //  * low.e() == w.e() == high.e()
    //  * low < w < high, and taking into account their error: low~ <= high~
    //  * kMinimalTargetExponent <= w.e() <= kMaximalTargetExponent
    // Postconditions: returns false if procedure fails.
    //   otherwise:
    //     * buffer is not null-terminated, but len contains the number of digits.
    //     * buffer contains the shortest possible decimal digit-sequence
    //       such that LOW < buffer * 10^kappa < HIGH, where LOW and HIGH are the
    //       correct values of low and high (without their error).
    //     * if more than one decimal representation gives the minimal number of
    //       decimal digits then the one closest to W (where W is the correct value
    //       of w) is chosen.
    // Remark: this procedure takes into account the imprecision of its input
    //   numbers. If the precision is not enough to guarantee all the postconditions
    //   then false is returned. This usually happens rarely (~0.5%).
    //
    // Say, for the sake of example, that
    //   w.e() == -48, and w.f() == 0x1234567890abcdef
    // w's value can be computed by w.f() * 2^w.e()
    // We can obtain w's integral digits by simply shifting w.f() by -w.e().
    //  -> w's integral part is 0x1234
    //  w's fractional part is therefore 0x567890abcdef.
    // Printing w's integral part is easy (simply print 0x1234 in decimal).
    // In order to print its fraction we repeatedly multiply the fraction by 10 and
    // get each digit. Example the first digit after the point would be computed by
    //   (0x567890abcdef * 10) >> 48. -> 3
    // The whole thing becomes slightly more complicated because we want to stop
    // once we have enough digits. That is, once the digits inside the buffer
    // represent 'w' we can stop. Everything inside the interval low - high
    // represents w. However we have to pay attention to low, high and w's
    // imprecision.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DigitGen(DiyFp       low,
                                 DiyFp       w,
                                 DiyFp       high,
                                 Span<char>  buffer,
                                 out int32_t length,
                                 out int32_t kappa) {
        length = 0;
        DOUBLE_CONVERSION_ASSERT(low.e() == w.e() && w.e() == high.e());
        DOUBLE_CONVERSION_ASSERT(low.f() + 1 <= high.f() - 1);
        DOUBLE_CONVERSION_ASSERT(w.e() is >= kMinimalTargetExponent and <= kMaximalTargetExponent);
        // low, w and high are imprecise, but by less than one ulp (unit in the last
        // place).
        // If we remove (resp. add) 1 ulp from low (resp. high) we are certain that
        // the new numbers are outside of the interval we want the final
        // representation to lie in.
        // Inversely adding (resp. removing) 1 ulp from low (resp. high) would yield
        // numbers that are certain to lie in the interval. We will use this fact
        // later on.
        // We will now start by generating the digits within the uncertain
        // interval. Later we will weed out representations that lie outside the safe
        // interval and thus _might_ lie outside the correct interval.
        uint64_t unit = 1;
        // var too_low = new DiyFp(low.f() - unit, low.e());
        // var too_high = new DiyFp(high.f() + unit, high.e());
        // too_low and too_high are guaranteed to lie outside the interval we want the
        // generated number in.
        uint64_t unsafe_interval_f = high.f() - low.f();
        // We now cut the input number into two parts: the integral digits and the
        // fractionals. We will not write any decimal separator though, but adapt
        // kappa instead.
        // Reminder: we are currently computing the digits (stored inside the buffer)
        // such that:   too_low < buffer * 10^kappa < too_high
        // We use too_high for the digit_generation and stop as soon as possible.
        // If we stop early we effectively round down.
        int32_t  one_e = -w.e();
        uint64_t one_f = (1UL << one_e) - 1;
        // Division by one is a shift.
        var integrals = (uint32_t)(high.f() >> one_e);
        // Modulo by one is an and.
        uint64_t fractionals = high.f() & one_f;
        BiggestPowerTen(integrals, DiyFp.kSignificandSize - one_e,
                        out _,     out kappa);
        ref uint32_t kSmallPowersOfTenRef = ref Utility.GetReference(kSmallPowersOfTen);

        // Loop invariant: buffer = too_high / 10^kappa  (integer division)
        // The invariant holds for the first iteration: kappa has been initialized
        // with the divisor exponent + 1. And the divisor is the biggest power of ten
        // that is smaller than integrals.
        for (; kappa > 0; kappa--) {
            (uint32_t digit, integrals) = System.Runtime.Intrinsics.X86.X86Base.DivRem(integrals, 0, Unsafe.Add(ref kSmallPowersOfTenRef, kappa));
            DOUBLE_CONVERSION_ASSERT(digit is >= 0 and <= 9);
            Utility.AssignAnyAt(buffer, length++, '0' + digit);
            // Note that kappa now equals the exponent of the divisor and that the
            // invariant thus holds again.
            uint64_t rest = ((uint64_t)integrals << one_e) + fractionals;
            // Invariant: too_high = buffer * 10^kappa + DiyFp(rest, one.e())
            // Reminder: unsafe_interval.e() == one.e()
            if (rest < unsafe_interval_f)
                // Rounding down (by not emitting the remaining digits) yields a number
                // that lies within the unsafe interval.
                return RoundWeed(buffer,                                                           length, high.f() - w.f(), unsafe_interval_f, rest,
                                 (uint64_t)Unsafe.Add(ref kSmallPowersOfTenRef, kappa--) << one_e, unit);
        }

        // The integrals have been generated. We are at the point of the decimal
        // separator. In the following loop we simply multiply the remaining digits by
        // 10 and divide by one. We just need to pay attention to multiply associated
        // data (like the interval or 'unit'), too.
        // Note that the multiplication by 10 does not overflow, because w.e >= -60
        // and thus one.e >= -60.
        DOUBLE_CONVERSION_ASSERT(one_e >= -60);
        DOUBLE_CONVERSION_ASSERT(fractionals < one_f);
        DOUBLE_CONVERSION_ASSERT(0xFFFFFFFFFFFFFFFF / 10 >= one_f);

        do {
            fractionals       *= 10;
            unit              *= 10;
            unsafe_interval_f *= 10;
            // Integer division by one.
            var digit = (int32_t)(fractionals >> one_e);
            DOUBLE_CONVERSION_ASSERT(digit is >= 0 and <= 9);
            Utility.AssignAnyAt(buffer, length++, '0' + digit);
            fractionals &= one_f; // Modulo by one.
            --kappa;
        } while (fractionals >= unsafe_interval_f);

        return RoundWeed(buffer,            length,      (high.f() - w.f()) * unit,
                         unsafe_interval_f, fractionals, one_f, unit);
    }


    // Generates (at most) requested_digits digits of input number w.
    // w is a floating-point number (DiyFp), consisting of a significand and an
    // exponent. Its exponent is bounded by kMinimalTargetExponent and
    // kMaximalTargetExponent.
    //       Hence -60 <= w.e() <= -32.
    //
    // Returns false if it fails, in which case the generated digits in the buffer
    // should not be used.
    // Preconditions:
    //  * w is correct up to 1 ulp (unit in the last place). That
    //    is, its error must be strictly less than a unit of its last digit.
    //  * kMinimalTargetExponent <= w.e() <= kMaximalTargetExponent
    //
    // Postconditions: returns false if procedure fails.
    //   otherwise:
    //     * buffer is not null-terminated, but length contains the number of
    //       digits.
    //     * the representation in buffer is the most precise representation of
    //       requested_digits digits.
    //     * buffer contains at most requested_digits digits of w. If there are less
    //       than requested_digits digits then some trailing '0's have been removed.
    //     * kappa is such that
    //            w = buffer * 10^kappa + eps with |eps| < 10^kappa / 2.
    //
    // Remark: This procedure takes into account the imprecision of its input
    //   numbers. If the precision is not enough to guarantee all the postconditions
    //   then false is returned. This usually happens rarely, but the failure-rate
    //   increases with higher requested_digits.
    public static bool DigitGenCounted(DiyFp      w,
                                        int32_t    requested_digits,
                                        Span<char> buffer,
                                        int32_t*   length,
                                        int32_t*   kappa) {
        DOUBLE_CONVERSION_ASSERT(w.e() is >= kMinimalTargetExponent and <= kMaximalTargetExponent);
        DOUBLE_CONVERSION_ASSERT(kMinimalTargetExponent >= -60);
        DOUBLE_CONVERSION_ASSERT(kMaximalTargetExponent <= -32);
        // w is assumed to have an error less than 1 unit. Whenever w is scaled we
        // also scale its error.
        uint64_t w_error = 1;
        // We cut the input number into two parts: the integral digits and the
        // fractional digits. We don't emit any decimal separator, but adapt kappa
        // instead. Example: instead of writing "1.2" we put "12" into the buffer and
        // increase kappa by 1.
        var one = new DiyFp(1UL << -w.e(), w.e());
        // Division by one is a shift.
        var integrals = (uint32_t)(w.f() >> -one.e());
        // Modulo by one is an and.
        uint64_t fractionals = w.f() & (one.f() - 1);
        BiggestPowerTen(integrals,            DiyFp.kSignificandSize - -one.e(),
                        out uint32_t divisor, out int32_t divisor_exponent_plus_one);
        *kappa  = divisor_exponent_plus_one;
        *length = 0;

        // Loop invariant: buffer = w / 10^kappa  (integer division)
        // The invariant holds for the first iteration: kappa has been initialized
        // with the divisor exponent + 1. And the divisor is the biggest power of ten
        // that is smaller than 'integrals'.
        // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
        // ReSharper cannot understand that *kappa will change LOL
        while (*kappa > 0) {
            int32_t digit = CastTo<int32_t, uint32_t>(integrals / divisor);
            DOUBLE_CONVERSION_ASSERT(digit is >= 0 and <= 9);
            buffer[*length] = (char)('0' + digit);
            (*length)++;
            requested_digits--;
            integrals %= divisor;
            (*kappa)--;
            // Note that kappa now equals the exponent of the divisor and that the
            // invariant thus holds again.
            if (requested_digits == 0) break;

            divisor /= 10;
        }

        if (requested_digits == 0) {
            uint64_t rest = ((uint64_t)integrals >> one.e()) + fractionals;
            return RoundWeedCounted(buffer,                       *length, rest,
                                    (uint64_t)divisor >> one.e(), w_error,
                                    kappa);
        }

        // The integrals have been generated. We are at the point of the decimal
        // separator. In the following loop we simply multiply the remaining digits by
        // 10 and divide by one. We just need to pay attention to multiply associated
        // data (the 'unit'), too.
        // Note that the multiplication by 10 does not overflow, because w.e >= -60
        // and thus one.e >= -60.
        DOUBLE_CONVERSION_ASSERT(one.e() >= -60);
        DOUBLE_CONVERSION_ASSERT(fractionals < one.f());
        DOUBLE_CONVERSION_ASSERT(0xFFFFFFFFFFFFFFFF / 10 >= one.f());

        while (requested_digits > 0 && fractionals > w_error) {
            fractionals *= 10;
            w_error     *= 10;
            // Integer division by one.
            var digit = (int32_t)(fractionals >> -one.e());
            DOUBLE_CONVERSION_ASSERT(digit is >= 0 and <= 9);
            buffer[*length] = (char)('0' + digit);
            (*length)++;
            requested_digits--;
            fractionals &= one.f() - 1; // Modulo by one.
            (*kappa)--;
        }

        return requested_digits == 0 && RoundWeedCounted(buffer, *length, fractionals, one.f(), w_error, kappa);
    }


    // Provides a decimal representation of v.
    // Returns true if it succeeds, otherwise the result cannot be trusted.
    // There will be *length digits inside the buffer (not null-terminated).
    // If the function returns true then
    //        v == (double) (buffer * 10^decimal_exponent).
    // The digits in the buffer are the shortest representation possible: no
    // 0.09999999999999999 instead of 0.1. The shorter representation will even be
    // chosen even if the longer one would be closer to v.
    // The last digit will be closest to the actual v. That is, even if several
    // digits might correctly yield 'v' when read again, the closest will be
    // computed.
    public static bool Grisu3(double       v,
                              FastDtoaMode mode,
                              Span<char>   buffer,
                              out int32_t  length,
                              out int32_t  decimal_exponent) {
        DiyFp w = new Double(v).AsNormalizedDiyFp();
        // boundary_minus and boundary_plus are the boundaries between v and its
        // closest floating-point neighbors. Any number strictly between
        // boundary_minus and boundary_plus will round to v when convert to a double.
        // Grisu3 will never output representations that lie exactly on a boundary.
        DiyFp boundary_minus, boundary_plus;

        if (mode == FastDtoaMode.FAST_DTOA_SHORTEST) {
            new Double(v).NormalizedBoundaries(out boundary_minus, out boundary_plus);
        } else {
            DOUBLE_CONVERSION_ASSERT(mode == FastDtoaMode.FAST_DTOA_SHORTEST_SINGLE);
            //float single_v = static_cast<float>(v);
            new Single((float)v).NormalizedBoundaries(out boundary_minus, out boundary_plus);
        }

        DOUBLE_CONVERSION_ASSERT(boundary_plus.e() == w.e());
        const int min_exp = kMinimalTargetExponent - DiyFp.kSignificandSize;
        const int max_exp = kMaximalTargetExponent - DiyFp.kSignificandSize;
        PowersOfTenCache.GetCachedPowerForBinaryExponentRange(
                                                              min_exp - w.e(),
                                                              max_exp - w.e(),
                                                              out DiyFp ten_mk, // Cached power of ten: 10^-k
                                                              out int32_t mk    // -k
                                                             );
        DOUBLE_CONVERSION_ASSERT(w.e() + ten_mk.e() +
                                     DiyFp.kSignificandSize is >= kMinimalTargetExponent and
                                                               <= kMaximalTargetExponent);
        // Note that ten_mk is only an approximation of 10^-k. A DiyFp only contains a
        // 64 bit significand and ten_mk is thus only precise up to 64 bits.

        // The DiyFp.Times procedure rounds its result, and ten_mk is approximated
        // too. The variable scaled_w (as well as scaled_boundary_minus/plus) are now
        // off by a small amount.
        // In fact: scaled_w - w*10^k < 1ulp (unit in the last place) of scaled_w.
        // In other words: let f = scaled_w.f() and e = scaled_w.e(), then
        //           (f-1) * 2^e < w*10^k < (f+1) * 2^e
        w.Multiply(ref ten_mk);
        DOUBLE_CONVERSION_ASSERT(w.e() ==
                                 boundary_plus.e() + ten_mk.e() + DiyFp.kSignificandSize);
        // In theory it would be possible to avoid some recomputations by computing
        // the difference between w and boundary_minus/plus (a power of 2) and to
        // compute scaled_boundary_minus/plus by subtracting/adding from
        // scaled_w. However the code becomes much less readable and the speed
        // enhancements are not terrific.
        boundary_minus.Multiply(ref ten_mk);
        boundary_plus.Multiply(ref ten_mk);

        // DigitGen will generate the digits of scaled_w. Therefore we have
        // v == (double) (scaled_w * 10^-mk).
        // Set decimal_exponent == -mk and pass it to DigitGen. If scaled_w is not an
        // integer than it will be updated. For instance if scaled_w == 1.23 then
        // the buffer will be filled with "123" and the decimal_exponent will be
        // decreased by 2.
        bool result = DigitGen(boundary_minus, w,          boundary_plus,
                               buffer,         out length, out decimal_exponent);
        decimal_exponent -= mk;
        return result;
    }


    // The "counted" version of grisu3 (see above) only generates requested_digits
    // number of digits. This version does not generate the shortest representation,
    // and with enough requested digits 0.1 will at some point print as 0.9999999...
    // Grisu3 is too imprecise for real halfway cases (1.5 will not work) and
    // therefore the rounding strategy for halfway cases is irrelevant.
    public static bool Grisu3Counted(double     v,
                                     int32_t    requested_digits,
                                     Span<char> buffer,
                                     int32_t*   length,
                                     int32_t*   decimal_exponent) {
        DiyFp w = new Double(v).AsNormalizedDiyFp();
        int32_t ten_mk_minimal_binary_exponent =
            kMinimalTargetExponent - (w.e() + DiyFp.kSignificandSize);
        int32_t ten_mk_maximal_binary_exponent =
            kMaximalTargetExponent - (w.e() + DiyFp.kSignificandSize);
        PowersOfTenCache.GetCachedPowerForBinaryExponentRange(
                                                              ten_mk_minimal_binary_exponent,
                                                              ten_mk_maximal_binary_exponent,
                                                              out DiyFp ten_mk, out int32_t mk);
        DOUBLE_CONVERSION_ASSERT(w.e() + ten_mk.e() +
                                     DiyFp.kSignificandSize is >= kMinimalTargetExponent and
                                                               <= kMaximalTargetExponent);
        // Note that ten_mk is only an approximation of 10^-k. A DiyFp only contains a
        // 64 bit significand and ten_mk is thus only precise up to 64 bits.

        // The DiyFp.Times procedure rounds its result, and ten_mk is approximated
        // too. The variable scaled_w (as well as scaled_boundary_minus/plus) are now
        // off by a small amount.
        // In fact: scaled_w - w*10^k < 1ulp (unit in the last place) of scaled_w.
        // In other words: let f = scaled_w.f() and e = scaled_w.e(), then
        //           (f-1) * 2^e < w*10^k < (f+1) * 2^e
        DiyFp scaled_w = DiyFp.Times(ref w, ref ten_mk);

        // We now have (double) (scaled_w * 10^-mk).
        // DigitGen will generate the first requested_digits digits of scaled_w and
        // return together with a kappa such that scaled_w ~= buffer * 10^kappa. (It
        // will not always be exactly the same since DigitGenCounted only produces a
        // limited number of digits.)
        int32_t kappa;
        bool result = DigitGenCounted(scaled_w, requested_digits,
                                      buffer,   length, &kappa);
        *decimal_exponent = -mk + kappa;
        return result;
    }


    /// <summary>
    ///     Provides a decimal representation of v.<br />
    ///     The result should be interpreted as buffer * 10^(point - length).<br />
    ///     <br />
    ///     Precondition:<br />
    ///     * v must be a strictly positive finite double.<br />
    ///     <br />
    ///     Returns true if it succeeds, otherwise the result can not be trusted.<br />
    ///     There will be *length digits inside the buffer followed by a null terminator.<br />
    ///     If the function returns true and mode equals<br />
    ///     - FAST_DTOA_SHORTEST, then<br />
    ///     the parameter requested_digits is ignored.<br />
    ///     The result satisfies<br />
    ///     v == (double) (buffer * 10^(point - length)).<br />
    ///     The digits in the buffer are the shortest representation possible. E.g.<br />
    ///     if 0.099999999999 and 0.1 represent the same double then "1" is returned<br />
    ///     with point = 0.<br />
    ///     The last digit will be closest to the actual v. That is, even if several<br />
    ///     digits might correctly yield 'v' when read again, the buffer will contain<br />
    ///     the one closest to v.<br />
    ///     - FAST_DTOA_PRECISION, then<br />
    ///     the buffer contains requested_digits digits.<br />
    ///     the difference v - (buffer * 10^(point-length)) is closest to zero for<br />
    ///     all possible representations of requested_digits digits.<br />
    ///     If there are two values that are equally close, then FastDtoa returns<br />
    ///     false.<br />
    ///     For both modes the buffer must be large enough to hold the result.<br />
    /// </summary>
    /// <param name="v"></param>
    /// <param name="mode"></param>
    /// <param name="buffer"></param>
    /// <param name="length"></param>
    /// <param name="decimal_point"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static bool FastDtoa(double       v,
                                FastDtoaMode mode,
                                Span<char>   buffer,
                                out int32_t  length,
                                out int32_t  decimal_point) {
        DOUBLE_CONVERSION_ASSERT(v > 0);
        DOUBLE_CONVERSION_ASSERT(!new Double(v).IsSpecial());
        bool result = mode switch {
            FastDtoaMode.FAST_DTOA_SHORTEST or FastDtoaMode.FAST_DTOA_SHORTEST_SINGLE => Grisu3(v, mode, buffer, out length, out decimal_point),
            //FastDtoaMode.FAST_DTOA_PRECISION => Grisu3Counted(v, requested_digits,
            //                                       buffer, (int*)AsPointer(ref length), &decimal_exponent),
            _ => throw new InvalidOperationException("wtf???????"),
        };
        if (result) decimal_point += length;
        // buffer[*length] = '\0';
        return result;
    }


    // We assume that doubles and uint64_t have the same endianness.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint64_t double_to_uint64(double d) => Unsafe.BitCast<double, uint64_t>(d);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double uint64_to_double(uint64_t d64) => Unsafe.BitCast<uint64_t, double>(d64);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint32_t float_to_uint32(float f) => Unsafe.BitCast<float, uint32_t>(f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float uint32_to_float(uint32_t d32) => Unsafe.BitCast<uint32_t, float>(d32);

    public sealed class AssertionError : Exception {
        public AssertionError(string message) : base(message) { }
        public AssertionError(string message, Exception innerException) : base(message, innerException) { }
    }

    // This "Do It Yourself Floating Point" class implements a floating-point number
    // with a uint64 significand and an int32_t exponent. Normalized DiyFp numbers will
    // have the most significant bit of the significand set.
    // Multiplication and Subtraction do not normalize their results.
    // DiyFp store only non-negative numbers and are not designed to contain special
    // doubles (NaN and Infinity).
    [DebuggerDisplay("{$\"e_ = {e_}, f_ = {f_}, (double) = {new double_conversion.Double(new double_conversion.DyiFp(f_, e_))}\"}")]
    public struct DiyFp {
        public const int32_t kSignificandSize = 64;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DiyFp() {
            this.f_ = 0;
            this.e_ = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DiyFp(uint64_t significand, int32_t exponent) {
            this.f_ = significand;
            this.e_ = exponent;
        }

        // this -= other.
        // The exponents of both numbers must be the same and the significand of this
        // must be greater or equal than the significand of other.
        // The result will not be normalized.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subtract(ref DiyFp other) {
            DOUBLE_CONVERSION_ASSERT(this.e_ == other.e_);
            DOUBLE_CONVERSION_ASSERT(this.f_ >= other.f_);
            this.f_ -= other.f_;
        }

        // Returns a - b.
        // The exponents of both numbers must be the same and a must be greater
        // or equal than b. The result will not be normalized.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DiyFp Minus(ref DiyFp a, ref DiyFp b) {
            DiyFp result = a;
            result.Subtract(ref b);
            return result;
        }

        // this *= other.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Multiply(ref DiyFp other) {
            // Simply "emulates" a 128 bit multiplication.
            // However: the resulting number only contains 64 bits. The least
            // significant 64 bits are only used for rounding the most significant 64
            // bits.
            //const uint64_t kM32 = 0xFFFF_FFFF;
            //uint64_t a = this.f_ >> 32;
            //uint64_t b = this.f_ & kM32;
            //uint64_t c = other.f_ >> 32;
            //uint64_t d = other.f_ & kM32;
            //uint64_t ac = a * c;
            //uint64_t bc = b * c;
            //uint64_t ad = a * d;
            //uint64_t bd = b * d;
            //// By adding 1U << 31 to tmp we round the final result.
            //// Halfway cases will be rounded up.
            //uint64_t tmp = (bd >> 32) + (ad & kM32) + (bc & kM32) + 0x8000_0000;
            //this.f_ = ac + (ad >> 32) + (bc >> 32) + (tmp >> 32);
            this.f_ =  Math.BigMul(this.f_, other.f_, out _);
            this.e_ += other.e_ + 64;
        }

        // returns a * b;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DiyFp Times(ref DiyFp a, ref DiyFp b) {
            DiyFp result = a;
            result.Multiply(ref b);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref DiyFp Normalize() {
            DOUBLE_CONVERSION_ASSERT(this.f_ != 0);
            int32_t lzcnt = System.Numerics.BitOperations.LeadingZeroCount(this.f_);
            //uint64_t significand = this.f_;
            //int32_t exponent = this.e_;

            //// This method is mainly called for normalizing boundaries. In general,
            //// boundaries need to be shifted by 10 bits, and we optimize for this case.
            //const uint64_t k10MSBits = 0xFFC0_0000_0000_0000;
            //while ((significand & k10MSBits) == 0) {
            //    significand <<= 10;
            //    exponent -= 10;
            //}
            //while ((significand & kUint64MSB) == 0) {
            //    significand <<= 1;
            //    exponent--;
            //}
            this.f_ <<= lzcnt;
            this.e_ -=  lzcnt;
            return ref this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DiyFp Normalize(in DiyFp a) {
            DiyFp result = a;
            _ = result.Normalize();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint64_t f() => this.f_;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int32_t e() => this.e_;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void set_f(uint64_t new_value) => this.f_ = new_value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void set_e(int32_t new_value) => this.e_ = new_value;

        public const uint64_t kUint64MSB = 0x8000000000000000;

        public uint64_t f_;
        public int32_t  e_;
    }

    public static class PowersOfTenCache {
        // Not all powers of ten are cached. The decimal exponent of two neighboring
        // cached numbers will differ by kDecimalExponentDistance.
        public const int32_t kDecimalExponentDistance = 8;

        public const int32_t kMinDecimalExponent = -348;
        public const int32_t kMaxDecimalExponent = 340;

        public const int32_t kCachedPowersOffset = 348;                 // -1 * the first decimal_exponent.
        public const double  kD_1_LOG2_10        = 0.30102999566398114; //  1 / log2(10)


        public static readonly CachedPower[] kCachedPowers = [
            new CachedPower(0xfa8fd5a0081c0288, -1220, -348),
            new CachedPower(0xbaaee17fa23ebf76, -1193, -340),
            new CachedPower(0x8b16fb203055ac76, -1166, -332),
            new CachedPower(0xcf42894a5dce35ea, -1140, -324),
            new CachedPower(0x9a6bb0aa55653b2d, -1113, -316),
            new CachedPower(0xe61acf033d1a45df, -1087, -308),
            new CachedPower(0xab70fe17c79ac6ca, -1060, -300),
            new CachedPower(0xff77b1fcbebcdc4f, -1034, -292),
            new CachedPower(0xbe5691ef416bd60c, -1007, -284),
            new CachedPower(0x8dd01fad907ffc3c, -980,  -276),
            new CachedPower(0xd3515c2831559a83, -954,  -268),
            new CachedPower(0x9d71ac8fada6c9b5, -927,  -260),
            new CachedPower(0xea9c227723ee8bcb, -901,  -252),
            new CachedPower(0xaecc49914078536d, -874,  -244),
            new CachedPower(0x823c12795db6ce57, -847,  -236),
            new CachedPower(0xc21094364dfb5637, -821,  -228),
            new CachedPower(0x9096ea6f3848984f, -794,  -220),
            new CachedPower(0xd77485cb25823ac7, -768,  -212),
            new CachedPower(0xa086cfcd97bf97f4, -741,  -204),
            new CachedPower(0xef340a98172aace5, -715,  -196),
            new CachedPower(0xb23867fb2a35b28e, -688,  -188),
            new CachedPower(0x84c8d4dfd2c63f3b, -661,  -180),
            new CachedPower(0xc5dd44271ad3cdba, -635,  -172),
            new CachedPower(0x936b9fcebb25c996, -608,  -164),
            new CachedPower(0xdbac6c247d62a584, -582,  -156),
            new CachedPower(0xa3ab66580d5fdaf6, -555,  -148),
            new CachedPower(0xf3e2f893dec3f126, -529,  -140),
            new CachedPower(0xb5b5ada8aaff80b8, -502,  -132),
            new CachedPower(0x87625f056c7c4a8b, -475,  -124),
            new CachedPower(0xc9bcff6034c13053, -449,  -116),
            new CachedPower(0x964e858c91ba2655, -422,  -108),
            new CachedPower(0xdff9772470297ebd, -396,  -100),
            new CachedPower(0xa6dfbd9fb8e5b88f, -369,  -92),
            new CachedPower(0xf8a95fcf88747d94, -343,  -84),
            new CachedPower(0xb94470938fa89bcf, -316,  -76),
            new CachedPower(0x8a08f0f8bf0f156b, -289,  -68),
            new CachedPower(0xcdb02555653131b6, -263,  -60),
            new CachedPower(0x993fe2c6d07b7fac, -236,  -52),
            new CachedPower(0xe45c10c42a2b3b06, -210,  -44),
            new CachedPower(0xaa242499697392d3, -183,  -36),
            new CachedPower(0xfd87b5f28300ca0e, -157,  -28),
            new CachedPower(0xbce5086492111aeb, -130,  -20),
            new CachedPower(0x8cbccc096f5088cc, -103,  -12),
            new CachedPower(0xd1b71758e219652c, -77,   -4),
            new CachedPower(0x9c40000000000000, -50,   4),
            new CachedPower(0xe8d4a51000000000, -24,   12),
            new CachedPower(0xad78ebc5ac620000, 3,     20),
            new CachedPower(0x813f3978f8940984, 30,    28),
            new CachedPower(0xc097ce7bc90715b3, 56,    36),
            new CachedPower(0x8f7e32ce7bea5c70, 83,    44),
            new CachedPower(0xd5d238a4abe98068, 109,   52),
            new CachedPower(0x9f4f2726179a2245, 136,   60),
            new CachedPower(0xed63a231d4c4fb27, 162,   68),
            new CachedPower(0xb0de65388cc8ada8, 189,   76),
            new CachedPower(0x83c7088e1aab65db, 216,   84),
            new CachedPower(0xc45d1df942711d9a, 242,   92),
            new CachedPower(0x924d692ca61be758, 269,   100),
            new CachedPower(0xda01ee641a708dea, 295,   108),
            new CachedPower(0xa26da3999aef774a, 322,   116),
            new CachedPower(0xf209787bb47d6b85, 348,   124),
            new CachedPower(0xb454e4a179dd1877, 375,   132),
            new CachedPower(0x865b86925b9bc5c2, 402,   140),
            new CachedPower(0xc83553c5c8965d3d, 428,   148),
            new CachedPower(0x952ab45cfa97a0b3, 455,   156),
            new CachedPower(0xde469fbd99a05fe3, 481,   164),
            new CachedPower(0xa59bc234db398c25, 508,   172),
            new CachedPower(0xf6c69a72a3989f5c, 534,   180),
            new CachedPower(0xb7dcbf5354e9bece, 561,   188),
            new CachedPower(0x88fcf317f22241e2, 588,   196),
            new CachedPower(0xcc20ce9bd35c78a5, 614,   204),
            new CachedPower(0x98165af37b2153df, 641,   212),
            new CachedPower(0xe2a0b5dc971f303a, 667,   220),
            new CachedPower(0xa8d9d1535ce3b396, 694,   228),
            new CachedPower(0xfb9b7cd9a4a7443c, 720,   236),
            new CachedPower(0xbb764c4ca7a44410, 747,   244),
            new CachedPower(0x8bab8eefb6409c1a, 774,   252),
            new CachedPower(0xd01fef10a657842c, 800,   260),
            new CachedPower(0x9b10a4e5e9913129, 827,   268),
            new CachedPower(0xe7109bfba19c0c9d, 853,   276),
            new CachedPower(0xac2820d9623bf429, 880,   284),
            new CachedPower(0x80444b5e7aa7cf85, 907,   292),
            new CachedPower(0xbf21e44003acdd2d, 933,   300),
            new CachedPower(0x8e679c2f5e44ff8f, 960,   308),
            new CachedPower(0xd433179d9c8cb841, 986,   316),
            new CachedPower(0x9e19db92b4e31ba9, 1013,  324),
            new CachedPower(0xeb96bf6ebadf77d9, 1039,  332),
            new CachedPower(0xaf87023b9bf0ee6b, 1066,  340),
        ];

        // Returns a cached power-of-ten with a binary exponent in the range
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // [min_exponent; max_exponent] (boundaries included).
        public static void GetCachedPowerForBinaryExponentRange(
            int32_t     min_exponent,
            int32_t     max_exponent,
            out DiyFp   power,
            out int32_t decimal_exponent) {
            const int32_t kQ    = DiyFp.kSignificandSize - 1;
            double        k     = Math.Ceiling((min_exponent + kQ) * kD_1_LOG2_10);
            int32_t       index = (kCachedPowersOffset + unchecked((int32_t)k) - 1) / kDecimalExponentDistance + 1;
            DOUBLE_CONVERSION_ASSERT(0 <= index && index < kCachedPowers.Length);
            CachedPower cached_power = Utility.GetReference(kCachedPowers, index);
            DOUBLE_CONVERSION_ASSERT(min_exponent <= cached_power.binary_exponent);
            DOUBLE_CONVERSION_ASSERT(cached_power.binary_exponent <= max_exponent);
            decimal_exponent = cached_power.decimal_exponent;
            power            = new DiyFp(cached_power.significand, cached_power.binary_exponent);
        }


        // Returns a cached power of ten x ~= 10^k such that
        //   k <= decimal_exponent < k + kCachedPowersDecimalDistance.
        // The given decimal_exponent must satisfy
        //   kMinDecimalExponent <= requested_exponent, and
        //   requested_exponent < kMaxDecimalExponent + kDecimalExponentDistance.
        public static void GetCachedPowerForDecimalExponent(int32_t  requested_exponent,
                                                            DiyFp*   power,
                                                            int32_t* found_exponent) {
            DOUBLE_CONVERSION_ASSERT(kMinDecimalExponent <= requested_exponent);
            DOUBLE_CONVERSION_ASSERT(requested_exponent < kMaxDecimalExponent + kDecimalExponentDistance);
            int32_t index =
                (requested_exponent + kCachedPowersOffset) / kDecimalExponentDistance;
            CachedPower cached_power = kCachedPowers[index];
            *power          = new DiyFp(cached_power.significand, cached_power.binary_exponent);
            *found_exponent = cached_power.decimal_exponent;
            DOUBLE_CONVERSION_ASSERT(*found_exponent <= requested_exponent);
            DOUBLE_CONVERSION_ASSERT(requested_exponent < *found_exponent + kDecimalExponentDistance);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public readonly record struct CachedPower(uint64_t significand, int16_t binary_exponent, int16_t decimal_exponent);
    } // namespace PowersOfTenCache

    // Helper functions for doubles.
    public readonly struct Double {
        public const uint64_t kSignMask                = 0x8000000000000000;
        public const uint64_t kExponentMask            = 0x7FF0000000000000;
        public const uint64_t kSignificandMask         = 0x000FFFFFFFFFFFFF;
        public const uint64_t kHiddenBit               = 0x0010000000000000;
        public const uint64_t kQuietNanBit             = 0x0008000000000000;
        public const int32_t  kPhysicalSignificandSize = 52; // Excludes the hidden bit.
        public const int32_t  kSignificandSize         = 53;
        public const int32_t  kExponentBias            = 0x3FF + kPhysicalSignificandSize;
        public const int32_t  kMaxExponent             = 0x7FF - kExponentBias;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Double() => this.d64_ = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Double(double d) => this.d64_ = double_to_uint64(d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Double(uint64_t d64) => this.d64_ = d64;

        public Double(DiyFp diy_fp) => this.d64_ = DiyFpToUint64(diy_fp);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Double(double d) => Unsafe.BitCast<double, Double>(d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Double(uint64_t d64) => Unsafe.BitCast<uint64_t, Double>(d64);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Double(DiyFp diy_fp) => Unsafe.BitCast<uint64_t, Double>(DiyFpToUint64(diy_fp));

        // The value encoded by this Double must be greater or equal to +0.0.
        // It must not be special (infinity, or NaN).
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DiyFp AsDiyFp() {
            DOUBLE_CONVERSION_ASSERT(this.Sign() > 0);
            DOUBLE_CONVERSION_ASSERT(!this.IsSpecial());
            return new DiyFp(this.Significand(), this.Exponent());
        }

        // The value encoded by this Double must be strictly greater than 0.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DiyFp AsNormalizedDiyFp() {
            DOUBLE_CONVERSION_ASSERT(this.value() > 0.0);
            uint64_t f    = this.Significand();
            int32_t  e    = this.Exponent();
            int32_t  f_lz = System.Numerics.BitOperations.LeadingZeroCount(f);

            // The current double could be a denormal.
            //while ((f & kHiddenBit) == 0) {
            //    f <<= 1;
            //    e--;
            //}
            // Do the final shifts in one go.
            return new DiyFp(f << f_lz, e - f_lz);
        }

        // Returns the double's bit as uint64.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint64_t AsUint64() => this.d64_;

        // Returns the next greater double. Returns +infinity on input +infinity.
        public double NextDouble() => this.d64_ == kInfinity
            ? new Double(kInfinity).value()
            : this.Sign() < 0 && this.Significand() == 0 // -0.0
                ? 0.0
                : this.Sign() < 0
                    ? new Double(this.d64_ - 1).value()
                    : new Double(this.d64_ + 1).value();

        public double PreviousDouble() => this.d64_ == (kInfinity | kSignMask)
            ? -Infinity()
            : this.Sign() < 0
                ? new Double(this.d64_ + 1).value()
                : this.Significand() == 0
                    ? -0.0
                    : new Double(this.d64_ - 1).value();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int32_t Exponent() => this.IsDenormal()
            ? kDenormalExponent
            : (int32_t)System.Runtime.Intrinsics.X86.Bmi1.X64.BitFieldExtract(this.AsUint64(), kPhysicalSignificandSize, 63) -
              kExponentBias; // biased_e

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint64_t Significand() => (this.AsUint64() & kSignificandMask) + kHiddenBit * Unsafe.BitCast<bool, byte>(!this.IsDenormal());

        // Returns true if the double is a denormal.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDenormal() => (this.AsUint64() & kExponentMask) == 0;

        // We consider denormals not to be special.
        // Hence only Infinity and NaN are special.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSpecial() => (this.AsUint64() & kExponentMask) == kExponentMask;

        public bool IsNan() {
            uint64_t d64 = this.AsUint64();
            return (d64 & kExponentMask) == kExponentMask &&
                   (d64 & kSignificandMask) != 0;
        }

        public bool IsQuietNan() => this.IsNan() && (this.AsUint64() & kQuietNanBit) != 0;

        public bool IsSignalingNan() => this.IsNan() && (this.AsUint64() & kQuietNanBit) == 0;


        public bool IsInfinite() {
            uint64_t d64 = this.AsUint64();
            return (d64 & kExponentMask) == kExponentMask &&
                   (d64 & kSignificandMask) == 0;
        }

        public int32_t Sign() {
            uint64_t d64 = this.AsUint64();
            return (d64 & kSignMask) == 0 ? 1 : -1;
        }

        // Precondition: the value encoded by this Double must be greater or equal
        // than +0.0.
        public DiyFp UpperBoundary() {
            DOUBLE_CONVERSION_ASSERT(this.Sign() > 0);
            return new DiyFp(this.Significand() * 2 + 1, this.Exponent() - 1);
        }

        // Computes the two boundaries of this.
        // The bigger boundary (m_plus) is normalized. The lower boundary has the same
        // exponent as m_plus.
        // Precondition: the value encoded by this Double must be greater than 0.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NormalizedBoundaries(out DiyFp out_m_minus, out DiyFp out_m_plus) {
            DOUBLE_CONVERSION_ASSERT(this.value() > 0.0);
            DiyFp v       = this.AsDiyFp();
            DiyFp m_plus  = new DiyFp((v.f() << 1) + 1,                                v.e() - 1).Normalize();
            DiyFp m_minus = this.LowerBoundaryIsCloser() ? new DiyFp((v.f() << 2) - 1, v.e() - 2) : new DiyFp((v.f() << 1) - 1, v.e() - 1);
            m_minus.set_f(m_minus.f() << m_minus.e() - m_plus.e());
            m_minus.set_e(m_plus.e());
            out_m_plus  = m_plus;
            out_m_minus = m_minus;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LowerBoundaryIsCloser() {
            // The boundary is closer if the significand is of the form f == 2^p-1 then
            // the lower boundary is closer.
            // Think of v = 1000e10 and v- = 9999e9.
            // Then the boundary (== (v - v-)/2) is not just at a distance of 1e9 but
            // at a distance of 1e8.
            // The only exception is for the smallest normal: the largest denormal is
            // at the same distance as its successor.
            // Note: denormals have the same exponent as the smallest normals.
            bool physical_significand_is_zero = (this.AsUint64() & kSignificandMask) == 0;
            return physical_significand_is_zero && this.Exponent() != kDenormalExponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double value() => uint64_to_double(this.d64_);

        // Returns the significand size for a given order of magnitude.
        // If v = f*2^e with 2^p-1 <= f <= 2^p then p+e is v's order of magnitude.
        // This function returns the number of significant binary digits v will have
        // once it's encoded into a double. In almost all cases this is equal to
        // kSignificandSize. The only exceptions are denormals. They start with
        // leading zeroes and their effective significand-size is hence smaller.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int32_t SignificandSizeForOrderOfMagnitude(int32_t order) => order >= kDenormalExponent + kSignificandSize ? kSignificandSize :
            order <= kDenormalExponent ? 0 : order - kDenormalExponent;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Infinity() => new Double(kInfinity).value();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double NaN() => new Double(kNaN).value();

        public const int32_t  kDenormalExponent = -kExponentBias + 1;
        public const uint64_t kInfinity         = 0x7FF0000000000000;
        public const uint64_t kNaN              = 0x7FF8000000000000;


        public readonly uint64_t d64_;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint64_t DiyFpToUint64(DiyFp diy_fp) {
            uint64_t significand = diy_fp.f();
            int32_t  exponent    = diy_fp.e();

            while (significand > kHiddenBit + kSignificandMask) {
                significand >>= 1;
                exponent++;
            }

            if (exponent >= kMaxExponent) return kInfinity;

            if (exponent < kDenormalExponent) return 0;

            while (exponent > kDenormalExponent && (significand & kHiddenBit) == 0) {
                significand <<= 1;
                exponent--;
            }

            uint64_t biased_exponent = exponent == kDenormalExponent && (significand & kHiddenBit) == 0 ? 0 : (uint64_t)(exponent + kExponentBias);
            return (significand & kSignificandMask) |
                   (biased_exponent << kPhysicalSignificandSize);
        }

        //DOUBLE_CONVERSION_DISALLOW_COPY_AND_ASSIGN(Double);
    }

    public struct Single {
        public const uint32_t kSignMask                = 0x80000000;
        public const uint32_t kExponentMask            = 0x7F800000;
        public const uint32_t kSignificandMask         = 0x007FFFFF;
        public const uint32_t kHiddenBit               = 0x00800000;
        public const uint32_t kQuietNanBit             = 0x00400000;
        public const int32_t  kPhysicalSignificandSize = 23; // Excludes the hidden bit.
        public const int32_t  kSignificandSize         = 24;

        public Single() => this.d32_ = 0;
        public Single(float                             f) => this.d32_ = float_to_uint32(f);
        public Single(uint32_t                          d32) => this.d32_ = d32;
        public static explicit operator Single(float    f)   => new () { d32_ = float_to_uint32(f) };
        public static explicit operator Single(uint32_t d32) => new () { d32_ = d32 };

        // The value encoded by this Single must be greater or equal to +0.0.
        // It must not be special (infinity, or NaN).
        public DiyFp AsDiyFp() {
            DOUBLE_CONVERSION_ASSERT(this.Sign() > 0);
            DOUBLE_CONVERSION_ASSERT(!this.IsSpecial());
            return new DiyFp(this.Significand(), this.Exponent());
        }

        // Returns the single's bit as uint64.
        public uint32_t AsUint32() => this.d32_;

        public int32_t Exponent() {
            if (this.IsDenormal()) return kDenormalExponent;

            uint32_t d32 = this.AsUint32();
            var biased_e =
                (int32_t)((d32 & kExponentMask) >> kPhysicalSignificandSize);
            return biased_e - kExponentBias;
        }

        public uint32_t Significand() {
            uint32_t d32         = this.AsUint32();
            uint32_t significand = d32 & kSignificandMask;
            return !this.IsDenormal() ? significand + kHiddenBit : significand;
        }

        // Returns true if the single is a denormal.
        public bool IsDenormal() => (this.AsUint32() & kExponentMask) == 0;

        // We consider denormals not to be special.
        // Hence only Infinity and NaN are special.
        public bool IsSpecial() => (this.AsUint32() & kExponentMask) == kExponentMask;

        public bool IsNan() {
            uint32_t d32 = this.AsUint32();
            return (d32 & kExponentMask) == kExponentMask &&
                   (d32 & kSignificandMask) != 0;
        }

        public bool IsQuietNan() => this.IsNan() && (this.AsUint32() & kQuietNanBit) != 0;

        public bool IsSignalingNan() => this.IsNan() && (this.AsUint32() & kQuietNanBit) == 0;


        public bool IsInfinite() {
            uint32_t d32 = this.AsUint32();
            return (d32 & kExponentMask) == kExponentMask &&
                   (d32 & kSignificandMask) == 0;
        }

        public int32_t Sign() {
            uint32_t d32 = this.AsUint32();
            return (d32 & kSignMask) == 0 ? 1 : -1;
        }

        // Computes the two boundaries of this.
        // The bigger boundary (m_plus) is normalized. The lower boundary has the same
        // exponent as m_plus.
        // Precondition: the value encoded by this Single must be greater than 0.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NormalizedBoundaries(out DiyFp out_m_minus, out DiyFp out_m_plus) {
            DOUBLE_CONVERSION_ASSERT(this.value() > 0.0);
            DiyFp v = this.AsDiyFp();
            out_m_plus = new DiyFp((v.f() << 1) + 1, v.e() - 1).Normalize();
            //int32_t n = CastTo<int32_t, bool>(this.LowerBoundaryIsCloser());
            //out_m_minus = new DiyFp((v.f() << (1 + n)) - 1 >> n, out_m_plus.e());
            out_m_minus    =   this.LowerBoundaryIsCloser() ? new DiyFp((v.f() << 2) - 1, v.e() - 2) : new DiyFp((v.f() << 1) - 1, v.e() - 1);
            out_m_minus.f_ <<= out_m_minus.e() - out_m_plus.e();
            out_m_minus.e_ =   out_m_plus.e();
        }

        // Precondition: the value encoded by this Single must be greater or equal
        // than +0.0.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DiyFp UpperBoundary() {
            DOUBLE_CONVERSION_ASSERT(this.Sign() > 0);
            return new DiyFp(this.Significand() * 2 + 1, this.Exponent() - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LowerBoundaryIsCloser() {
            // The boundary is closer if the significand is of the form f == 2^p-1 then
            // the lower boundary is closer.
            // Think of v = 1000e10 and v- = 9999e9.
            // Then the boundary (== (v - v-)/2) is not just at a distance of 1e9 but
            // at a distance of 1e8.
            // The only exception is for the smallest normal: the largest denormal is
            // at the same distance as its successor.
            // Note: denormals have the same exponent as the smallest normals.
            bool physical_significand_is_zero = (this.AsUint32() & kSignificandMask) == 0;
            return physical_significand_is_zero && this.Exponent() != kDenormalExponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float value() => uint32_to_float(this.d32_);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Infinity() => new Single(kInfinity).value();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NaN() => new Single(kNaN).value();

        public const int32_t  kExponentBias     = 0x7F + kPhysicalSignificandSize;
        public const int32_t  kDenormalExponent = -kExponentBias + 1;
        public const int32_t  kMaxExponent      = 0xFF - kExponentBias;
        public const uint32_t kInfinity         = 0x7F800000;
        public const uint32_t kNaN              = 0x7FC00000;

        public uint32_t d32_;

        //DOUBLE_CONVERSION_DISALLOW_COPY_AND_ASSIGN(Single);
    }
} // namespace double_conversion
