import type { Meta, StoryObj } from '@storybook/react';
import { Pagination } from './pagination';

const meta = {
  component: Pagination,
  title: 'Molecules/Pagination',
  argTypes: {
    page: { control: 'number' },
    totalPages: { control: 'number' },
    total: { control: 'number' },
  },
  args: {
    page: 1,
    totalPages: 5,
    total: 100,
  },
} satisfies Meta<typeof Pagination>;
export default meta;

type Story = StoryObj<typeof Pagination>;

export const FirstPage: Story = {
  args: {
    page: 1,
    prevHref: undefined,
    nextHref: '?page=2',
  },
};

export const MiddlePage: Story = {
  args: {
    page: 3,
    prevHref: '?page=2',
    nextHref: '?page=4',
  },
};

export const LastPage: Story = {
  args: {
    page: 5,
    prevHref: '?page=4',
    nextHref: undefined,
  },
};

export const SingleResult: Story = {
  args: {
    page: 1,
    totalPages: 1,
    total: 1,
    prevHref: undefined,
    nextHref: undefined,
  },
};
