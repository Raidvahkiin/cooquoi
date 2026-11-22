'use client';

import { Button } from '@astryxdesign/core/Button';
import { HStack } from '@astryxdesign/core/Stack';
import { TextInput } from '@astryxdesign/core/TextInput';
import { useTranslations } from 'next-intl';
import { useState } from 'react';

type IngredientSearchFormProps = {
  query: string;
  /** Kept across searches so a new query does not reset the chosen sort. */
  sort?: string;
};

/** GET form: the query lives in the URL, so searching works without JavaScript. */
export function IngredientSearchForm({
  query,
  sort,
}: IngredientSearchFormProps) {
  const t = useTranslations('foodpedia.search');
  const [value, setValue] = useState(query);
  return (
    <search>
      <form action="/foodpedia">
        <HStack gap={2} align="end">
          <TextInput
            type="search"
            htmlName="q"
            label={t('label')}
            value={value}
            onChange={setValue}
            width="100%"
          />
          {sort && <input type="hidden" name="sort" value={sort} />}
          <Button type="submit" label={t('submit')} variant="primary" />
        </HStack>
      </form>
    </search>
  );
}
