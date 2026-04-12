import type { Meta, StoryObj } from '@storybook/react-vite';
import { Topbar  } from './top-bar';

const meta = {
  component: Topbar,
  title: 'Topbar',
} satisfies Meta<typeof Topbar>;
export default meta;

type Story = StoryObj<typeof Topbar>;

export const Primary = {
  args: {
  },
} satisfies Story;

