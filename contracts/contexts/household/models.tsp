import "../../common/main.tsp";

using Cooquoi.Common;

namespace Cooquoi.Household;

/** Role of a user in a household. */
union MembershipRole {
  owner: "owner",
  member: "member",
}

/** What Cooquoi stores about the signed-in user (ADR 0015). */
model UserProfile {
  userId: Id;
  displayName: string;
  email: string | null;
  locale: Locale;
  defaultHouseholdId: Id;
}

/** A household the user belongs to. */
model HouseholdSummary {
  id: Id;

  /** Null until renamed; clients display a translated default ("My household"). */
  name: string | null;

  role: MembershipRole;
}

/** The signed-in user's profile and households. */
model Me {
  profile: UserProfile;
  households: HouseholdSummary[];
}
