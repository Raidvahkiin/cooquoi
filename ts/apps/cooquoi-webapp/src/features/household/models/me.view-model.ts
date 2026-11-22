import type { HouseholdMe } from '@cooquoi/api-client';

export type MeViewModel = {
  displayName: string;
  email?: string;
  locale: string;
  householdCount: number;
};

export function toMeViewModel(me: HouseholdMe): MeViewModel {
  return {
    displayName: me.profile.displayName,
    email: me.profile.email ?? undefined,
    locale: me.profile.locale,
    householdCount: me.households.length,
  };
}
