#if FALSE
using uint64_t = System.UInt64;
using int32_t = System.Int32;

public static class dragonbox {
    public record struct ReturnType(uint64_t significand, int32_t exponent);

    static ReturnType compute_nearest_normal(uint64_t two_fc, int exponent) {
        //////////////////////////////////////////////////////////////////////
        // Step 1: Schubfach multiplier calculation
        //////////////////////////////////////////////////////////////////////

        ReturnType ret_value;
        // IntervalType interval_type;

        // Compute k and beta.
        int minus_k = log.floor_log10_pow2(exponent) - kappa;
        var cache = CachePolicy.get_cache<format>(-minus_k);
        int  beta = exponent + log.floor_log2_pow10(-minus_k);

        // Compute zi and deltai.
        // 10^kappa <= deltai < 10^(kappa + 1)
        var deltai = compute_delta(cache, beta);
        // For the case of binary32, the result of integer check is not correct for
        // 29711844 * 2^-82
        // = 6.1442653300000000008655037797566933477355632930994033813476... * 10^-18
        // and 29711844 * 2^-81
        // = 1.2288530660000000001731007559513386695471126586198806762695... * 10^-17,
        // and they are the unique counterexamples. However, since 29711844 is even,
        // this does not cause any problem for the endpoints calculations; it can only
        // cause a problem when we need to perform integer check for the center.
        // Fortunately, with these inputs, that branch is never executed, so we are fine.
        var (zi, is_z_integer) = compute_mul((two_fc | 1) << beta, cache);


        //////////////////////////////////////////////////////////////////////
        // Step 2: Try larger divisor; remove trailing zeros if necessary
        //////////////////////////////////////////////////////////////////////

        var big_divisor = compute_power(kappa + 1, (10));
        var small_divisor = compute_power(kappa, (10));

        // Using an upper bound on zi, we might be able to optimize the division
        // better than the compiler; we are computing zi / big_divisor here.
        ret_value.significand =
            div.divide_by_pow10(kappa + 1, carrier_uint,
                                    (1ul << (significand_bits + 1)) * big_divisor -
                                        1, zi);
        var r = (uint32_t)(zi - big_divisor * ret_value.significand);

        do {
            if (r < deltai) {
                // Exclude the right endpoint if necessary.
                // if (r == 0 && (is_z_integer & !interval_type.include_right_endpoint())) {
                //     // if constexpr (BinaryToDecimalRoundingPolicy::tag ==
                //     //               policy_impl::binary_to_decimal_rounding::tag_t::
                //     //                   do_not_care) {
                //     //     ret_value.significand *= 10;
                //     //     ret_value.exponent = minus_k + kappa;
                //     //     --ret_value.significand;
                //     //     TrailingZeroPolicy::template no_trailing_zeros<impl>(ret_value);
                //     //     return ret_value;
                //     // }
                //     // else {
                //         --ret_value.significand;
                //         r = big_divisor;
                //         break;
                //     // }
                // }
            }
            else if (r > deltai) {
                break;
            }
            else {
                // r == deltai; compare fractional parts.
                var (xi_parity, x_is_integer) =
                    compute_mul_parity(two_fc - 1, cache, beta);

                if (!(xi_parity | (x_is_integer))) {
                    break;
                }
            }
            ret_value.exponent = minus_k + kappa + 1;

            // We may need to remove trailing zeros.
            // TrailingZeroPolicy::template on_trailing_zeros<impl>(ret_value);
            return ret_value;
        } while (false);


        //////////////////////////////////////////////////////////////////////
        // Step 3: Find the significand with the smaller divisor
        //////////////////////////////////////////////////////////////////////

        // TrailingZeroPolicy::template no_trailing_zeros<impl>(ret_value);
        ret_value.significand *= 10;
        ret_value.exponent = minus_k + kappa;

        // if constexpr (BinaryToDecimalRoundingPolicy::tag ==
        //               policy_impl::binary_to_decimal_rounding::tag_t::do_not_care) {
        //     // Normally, we want to compute
        //     // ret_value.significand += r / small_divisor
        //     // and return, but we need to take care of the case that the resulting
        //     // value is exactly the right endpoint, while that is not included in the
        //     // interval.
        //     if (!interval_type.include_right_endpoint()) {
        //         // Is r divisible by 10^kappa?
        //         if (is_z_integer && div::check_divisibility_and_divide_by_pow10<kappa>(r)) {
        //             // This should be in the interval.
        //             ret_value.significand += r - 1;
        //         }
        //         else {
        //             ret_value.significand += r;
        //         }
        //     }
        //     else {
        //         ret_value.significand += div::small_division_by_pow10<kappa>(r);
        //     }
        // }
        // else {
            var dist = r - (deltai / 2) + (small_divisor / 2);
            bool approx_y_parity = ((dist ^ (small_divisor / 2)) & 1) != 0;

            // Is dist divisible by 10^kappa?
            bool divisible_by_small_divisor =
                div::check_divisibility_and_divide_by_pow10<kappa>(dist);

            // Add dist / 10^kappa to the significand.
            ret_value.significand += dist;

            if (divisible_by_small_divisor) {
                // Check z^(f) >= epsilon^(f).
                // We have either yi == zi - epsiloni or yi == (zi - epsiloni) - 1,
                // where yi == zi - epsiloni if and only if z^(f) >= epsilon^(f).
                // Since there are only 2 possibilities, we only need to care about the
                // parity. Also, zi and r should have the same parity since the divisor is
                // an even number.
                var (yi_parity, is_y_integer) =
                    compute_mul_parity(two_fc, cache, beta);
                if (yi_parity != approx_y_parity) {
                    --ret_value.significand;
                }
                else {
                    // If z^(f) >= epsilon^(f), we might have a tie
                    // when z^(f) == epsilon^(f), or equivalently, when y is an integer.
                    // For tie-to-up case, we can just choose the upper one.
                    if (ret_value.significand & 1 &
                        is_y_integer) {
                        --ret_value.significand;
                    }
                }
            }
        // }
        return ret_value;
    }

    static ReturnType compute_nearest_shorter(int exponent) {
        ReturnType ret_value;
        // IntervalType interval_type;

        // Compute k and beta.
        int minus_k = log.floor_log10_pow2_minus_log10_4_over_3(exponent);
        int beta = exponent + log.floor_log2_pow10(-minus_k);

        // Compute xi and zi.
        var cache = CachePolicy.get_cache<format>(-minus_k);

        var xi = compute_left_endpoint_for_shorter_interval_case(cache, beta);
        var zi = compute_right_endpoint_for_shorter_interval_case(cache, beta);

        // If we don't accept the right endpoint and
        // if the right endpoint is an integer, decrease it.
        // if (!interval_type.include_right_endpoint() &&
        //     is_right_endpoint_integer_shorter_interval(exponent)) {
        //     --zi;
        // }
        // If we don't accept the left endpoint or
        // if the left endpoint is not an integer, increase it.
        // if (!interval_type.include_left_endpoint() ||
        //     !is_left_endpoint_integer_shorter_interval(exponent)) {
        //     ++xi;
        // }

        // Try bigger divisor.
        ret_value.significand = zi / 10;

        // If succeed, remove trailing zeros if necessary and return.
        if (ret_value.significand * 10 >= xi) {
            ret_value.exponent = minus_k + 1;
            // TrailingZeroPolicy::template on_trailing_zeros<impl>(ret_value);
            return ret_value;
        }

        // Otherwise, compute the round-up of y.
        // TrailingZeroPolicy::template no_trailing_zeros<impl>(ret_value);
        ret_value.significand = compute_round_up_for_shorter_interval_case(cache, beta);
        ret_value.exponent = minus_k;

        // When tie occurs, choose one of them according to the rule.
        if (ret_value.significand & 1 &&
            exponent >= shorter_interval_tie_lower_threshold &&
            exponent <= shorter_interval_tie_upper_threshold) {
            --ret_value.significand;
        } else if (ret_value.significand < xi) {
            ++ret_value.significand;
        }
        return ret_value;
    }
}
#endif