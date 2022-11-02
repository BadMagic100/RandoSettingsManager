namespace RandoSettingsManager.SettingsManagement.Versioning
{
    /// <summary>
    /// A versioning policy that compounds versions and rules from multiple versioning policies
    /// </summary>
    public class CompoundVersioningPolicy<T1, T2> : VersioningPolicy<(T1, T2)>
    {
        private readonly (VersioningPolicy<T1>, VersioningPolicy<T2>) policies;

        /// <inheritdoc/>
        public override (T1, T2) Version => (policies.Item1.Version, policies.Item2.Version);

        internal CompoundVersioningPolicy(VersioningPolicy<T1> v1, VersioningPolicy<T2> v2)
        {
            policies = (v1, v2);
        }

        /// <inheritdoc/>
        public override bool Allow((T1, T2) version)
        {
            return policies.Item1.Allow(version.Item1) && policies.Item2.Allow(version.Item2);
        }
    }

    /// <summary>
    /// A versioning policy that compounds versions and rules from multiple versioning policies
    /// </summary>
    public class CompoundVersioningPolicy<T1, T2, T3> : VersioningPolicy<(T1, T2, T3)>
    {
        private readonly (VersioningPolicy<T1>, VersioningPolicy<T2>, VersioningPolicy<T3>) policies;

        /// <inheritdoc/>
        public override (T1, T2, T3) Version => (policies.Item1.Version, policies.Item2.Version, policies.Item3.Version);

        internal CompoundVersioningPolicy(VersioningPolicy<T1> v1, VersioningPolicy<T2> v2, VersioningPolicy<T3> v3)
        {
            policies = (v1, v2, v3);
        }

        /// <inheritdoc/>
        public override bool Allow((T1, T2, T3) version)
        {
            return policies.Item1.Allow(version.Item1) && policies.Item2.Allow(version.Item2)
                && policies.Item3.Allow(version.Item3);
        }
    }

    /// <summary>
    /// A versioning policy that compounds versions and rules from multiple versioning policies
    /// </summary>
    public class CompoundVersioningPolicy<T1, T2, T3, T4> : VersioningPolicy<(T1, T2, T3, T4)>
    {
        private readonly (VersioningPolicy<T1>, VersioningPolicy<T2>, 
            VersioningPolicy<T3>, VersioningPolicy<T4>) policies;

        /// <inheritdoc/>
        public override (T1, T2, T3, T4) Version => (policies.Item1.Version, policies.Item2.Version,
            policies.Item3.Version, policies.Item4.Version);

        internal CompoundVersioningPolicy(VersioningPolicy<T1> v1, VersioningPolicy<T2> v2, 
            VersioningPolicy<T3> v3, VersioningPolicy<T4> v4)
        {
            policies = (v1, v2, v3, v4);
        }

        /// <inheritdoc/>
        public override bool Allow((T1, T2, T3, T4) version)
        {
            return policies.Item1.Allow(version.Item1) && policies.Item2.Allow(version.Item2)
                && policies.Item3.Allow(version.Item3) && policies.Item4.Allow(version.Item4);
        }
    }

    /// <summary>
    /// A versioning policy that compounds versions and rules from multiple versioning policies
    /// </summary>
    public class CompoundVersioningPolicy<T1, T2, T3, T4, T5> : VersioningPolicy<(T1, T2, T3, T4, T5)>
    {
        private readonly (VersioningPolicy<T1>, VersioningPolicy<T2>,
            VersioningPolicy<T3>, VersioningPolicy<T4>, VersioningPolicy<T5>) policies;

        /// <inheritdoc/>
        public override (T1, T2, T3, T4, T5) Version => (policies.Item1.Version, policies.Item2.Version,
            policies.Item3.Version, policies.Item4.Version, policies.Item5.Version);

        internal CompoundVersioningPolicy(VersioningPolicy<T1> v1, VersioningPolicy<T2> v2,
            VersioningPolicy<T3> v3, VersioningPolicy<T4> v4, VersioningPolicy<T5> v5)
        {
            policies = (v1, v2, v3, v4, v5);
        }

        /// <inheritdoc/>
        public override bool Allow((T1, T2, T3, T4, T5) version)
        {
            return policies.Item1.Allow(version.Item1) && policies.Item2.Allow(version.Item2)
                && policies.Item3.Allow(version.Item3) && policies.Item4.Allow(version.Item4)
                && policies.Item5.Allow(version.Item5);
        }
    }

    /// <summary>
    /// A versioning policy that compounds versions and rules from multiple versioning policies
    /// </summary>
    public class CompoundVersioningPolicy<T1, T2, T3, T4, T5, T6>
        : VersioningPolicy<(T1, T2, T3, T4, T5, T6)>
    {
        private readonly (VersioningPolicy<T1>, VersioningPolicy<T2>,
            VersioningPolicy<T3>, VersioningPolicy<T4>, VersioningPolicy<T5>,
            VersioningPolicy<T6>) policies;

        /// <inheritdoc/>
        public override (T1, T2, T3, T4, T5, T6) Version =>
            (policies.Item1.Version, policies.Item2.Version, policies.Item3.Version,
            policies.Item4.Version, policies.Item5.Version, policies.Item6.Version);

        internal CompoundVersioningPolicy(VersioningPolicy<T1> v1, VersioningPolicy<T2> v2,
            VersioningPolicy<T3> v3, VersioningPolicy<T4> v4, VersioningPolicy<T5> v5,
            VersioningPolicy<T6> v6)
        {
            policies = (v1, v2, v3, v4, v5, v6);
        }

        /// <inheritdoc/>
        public override bool Allow((T1, T2, T3, T4, T5, T6) version)
        {
            return policies.Item1.Allow(version.Item1) && policies.Item2.Allow(version.Item2)
                && policies.Item3.Allow(version.Item3) && policies.Item4.Allow(version.Item4)
                && policies.Item5.Allow(version.Item5) && policies.Item6.Allow(version.Item6);
        }
    }

    /// <summary>
    /// A versioning policy that compounds versions and rules from multiple versioning policies
    /// </summary>
    public class CompoundVersioningPolicy<T1, T2, T3, T4, T5, T6, T7>
        : VersioningPolicy<(T1, T2, T3, T4, T5, T6, T7)>
    {
        private readonly (VersioningPolicy<T1>, VersioningPolicy<T2>,
            VersioningPolicy<T3>, VersioningPolicy<T4>, VersioningPolicy<T5>,
            VersioningPolicy<T6>, VersioningPolicy<T7>) policies;

        /// <inheritdoc/>
        public override (T1, T2, T3, T4, T5, T6, T7) Version =>
            (policies.Item1.Version, policies.Item2.Version, policies.Item3.Version,
            policies.Item4.Version, policies.Item5.Version, policies.Item6.Version,
            policies.Item7.Version);

        internal CompoundVersioningPolicy(VersioningPolicy<T1> v1, VersioningPolicy<T2> v2,
            VersioningPolicy<T3> v3, VersioningPolicy<T4> v4, VersioningPolicy<T5> v5,
            VersioningPolicy<T6> v6, VersioningPolicy<T7> v7)
        {
            policies = (v1, v2, v3, v4, v5, v6, v7);
        }

        /// <inheritdoc/>
        public override bool Allow((T1, T2, T3, T4, T5, T6, T7) version)
        {
            return policies.Item1.Allow(version.Item1) && policies.Item2.Allow(version.Item2)
                && policies.Item3.Allow(version.Item3) && policies.Item4.Allow(version.Item4)
                && policies.Item5.Allow(version.Item5) && policies.Item6.Allow(version.Item6)
                && policies.Item7.Allow(version.Item7);
        }
    }

    /// <summary>
    /// A versioning policy that compounds versions and rules from multiple versioning policies
    /// </summary>
    public class CompoundVersioningPolicy<T1, T2, T3, T4, T5, T6, T7, T8>
        : VersioningPolicy<(T1, T2, T3, T4, T5, T6, T7, T8)>
    {
        private readonly (VersioningPolicy<T1>, VersioningPolicy<T2>,
            VersioningPolicy<T3>, VersioningPolicy<T4>, VersioningPolicy<T5>,
            VersioningPolicy<T6>, VersioningPolicy<T7>, VersioningPolicy<T8>) policies;

        /// <inheritdoc/>
        public override (T1, T2, T3, T4, T5, T6, T7, T8) Version =>
            (policies.Item1.Version, policies.Item2.Version, policies.Item3.Version,
            policies.Item4.Version, policies.Item5.Version, policies.Item6.Version,
            policies.Item7.Version, policies.Item8.Version);

        internal CompoundVersioningPolicy(VersioningPolicy<T1> v1, VersioningPolicy<T2> v2,
            VersioningPolicy<T3> v3, VersioningPolicy<T4> v4, VersioningPolicy<T5> v5,
            VersioningPolicy<T6> v6, VersioningPolicy<T7> v7, VersioningPolicy<T8> v8)
        {
            policies = (v1, v2, v3, v4, v5, v6, v7, v8);
        }

        /// <inheritdoc/>
        public override bool Allow((T1, T2, T3, T4, T5, T6, T7, T8) version)
        {
            return policies.Item1.Allow(version.Item1) && policies.Item2.Allow(version.Item2)
                && policies.Item3.Allow(version.Item3) && policies.Item4.Allow(version.Item4)
                && policies.Item5.Allow(version.Item5) && policies.Item6.Allow(version.Item6)
                && policies.Item7.Allow(version.Item7) && policies.Item8.Allow(version.Item8);
        }
    }

    /// <summary>
    /// Class containing static convenience methods to create compound versioning policies
    /// </summary>
    public static class CompoundVersioningPolicy
    {
        /// <summary>
        /// Constructs a compound versioning policy from 2 versioning policies
        /// </summary>
        public static CompoundVersioningPolicy<T1, T2> Of<T1, T2>(VersioningPolicy<T1> v1, VersioningPolicy<T2> v2)
        {
            return new CompoundVersioningPolicy<T1, T2>(v1, v2);
        }

        /// <summary>
        /// Constructs a compound versioning policy from 3 versioning policies
        /// </summary>
        public static CompoundVersioningPolicy<T1, T2, T3> Of<T1, T2, T3>(VersioningPolicy<T1> v1, VersioningPolicy<T2> v2,
            VersioningPolicy<T3> v3)
        {
            return new CompoundVersioningPolicy<T1, T2, T3>(v1, v2, v3);
        }

        /// <summary>
        /// Constructs a compound versioning policy from 4 versioning policies
        /// </summary>
        public static CompoundVersioningPolicy<T1, T2, T3, T4> Of<T1, T2, T3, T4>(
            VersioningPolicy<T1> v1, VersioningPolicy<T2> v2, VersioningPolicy<T3> v3,
            VersioningPolicy<T4> v4)
        {
            return new CompoundVersioningPolicy<T1, T2, T3, T4>(v1, v2, v3, v4);
        }

        /// <summary>
        /// Constructs a compound versioning policy from 5 versioning policies
        /// </summary>
        public static CompoundVersioningPolicy<T1, T2, T3, T4, T5> Of<T1, T2, T3, T4, T5>(
            VersioningPolicy<T1> v1, VersioningPolicy<T2> v2, VersioningPolicy<T3> v3,
            VersioningPolicy<T4> v4, VersioningPolicy<T5> v5)
        {
            return new CompoundVersioningPolicy<T1, T2, T3, T4, T5>(v1, v2, v3, v4, v5);
        }

        /// <summary>
        /// Constructs a compound versioning policy from 6 versioning policies
        /// </summary>
        public static CompoundVersioningPolicy<T1, T2, T3, T4, T5, T6> Of<T1, T2, T3, T4, T5, T6>(
            VersioningPolicy<T1> v1, VersioningPolicy<T2> v2, VersioningPolicy<T3> v3,
            VersioningPolicy<T4> v4, VersioningPolicy<T5> v5, VersioningPolicy<T6> v6)
        {
            return new CompoundVersioningPolicy<T1, T2, T3, T4, T5, T6>(v1, v2, v3, v4, v5, v6);
        }

        /// <summary>
        /// Constructs a compound versioning policy from 7 versioning policies
        /// </summary>
        public static CompoundVersioningPolicy<T1, T2, T3, T4, T5, T6, T7> Of<T1, T2, T3, T4, T5, T6, T7>(
            VersioningPolicy<T1> v1, VersioningPolicy<T2> v2, VersioningPolicy<T3> v3,
            VersioningPolicy<T4> v4, VersioningPolicy<T5> v5, VersioningPolicy<T6> v6,
            VersioningPolicy<T7> v7)
        {
            return new CompoundVersioningPolicy<T1, T2, T3, T4, T5, T6, T7>(v1, v2, v3, v4, v5, v6, v7);
        }

        /// <summary>
        /// Constructs a compound versioning policy from 8 versioning policies
        /// </summary>
        public static CompoundVersioningPolicy<T1, T2, T3, T4, T5, T6, T7, T8> Of<T1, T2, T3, T4, T5, T6, T7, T8>(
            VersioningPolicy<T1> v1, VersioningPolicy<T2> v2, VersioningPolicy<T3> v3,
            VersioningPolicy<T4> v4, VersioningPolicy<T5> v5, VersioningPolicy<T6> v6,
            VersioningPolicy<T7> v7, VersioningPolicy<T8> v8)
        {
            return new CompoundVersioningPolicy<T1, T2, T3, T4, T5, T6, T7, T8>(v1, v2, v3, v4, v5, v6, v7, v8);
        }
    }
}
