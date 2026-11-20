import type { Meta, StoryObj } from '@storybook/react';
import { Button } from './button';
import { Modal, type ModalProps, useModal } from './modal';

const meta: Meta<typeof Modal> = {
  component: Modal,
  title: 'Modal',
  argTypes: {
    size: {
      control: 'inline-radio',
      options: ['sm', 'md', 'lg', 'xl', 'full'],
    },
    closeOnOverlayClick: { control: 'boolean' },
    showCloseButton: { control: 'boolean' },
  },
  args: {
    open: true,
    title: 'Modal Title',
    size: 'md',
    closeOnOverlayClick: true,
    showCloseButton: true,
    children: (
      <p className="text-sm text-neutral-600">
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque
        euismod, nisi eu consectetur. Sed do eiusmod tempor incididunt ut labore
        et dolore magna aliqua.
      </p>
    ),
  },
  decorators: [
    (Story) => (
      <div className="min-h-100">
        <Story />
      </div>
    ),
  ],
} satisfies Meta<typeof Modal>;
export default meta;

type Story = StoryObj<typeof Modal>;

export const Default: Story = {};

export const Small: Story = {
  args: { size: 'sm', title: 'Small Modal' },
};

export const Large: Story = {
  args: { size: 'lg', title: 'Large Modal' },
};

export const NoTitle: Story = {
  args: { title: undefined },
};

export const NoCloseButton: Story = {
  args: { showCloseButton: false },
};

export const WithFooter: Story = {
  args: {
    title: 'Confirm Action',
    children: (
      <p className="text-sm text-neutral-600">
        Are you sure you want to proceed?
      </p>
    ),
    footer: (
      <>
        <Button variant="outlined" color="tertiary">
          Cancel
        </Button>
        <Button variant="contained" color="primary">
          Confirm
        </Button>
      </>
    ),
  },
};

const InteractiveTemplate = (args: ModalProps) => {
  const { open, openModal, closeModal } = useModal();
  return (
    <>
      <Button variant="contained" color="primary" onClick={openModal}>
        Open Modal
      </Button>
      <Modal {...args} open={open} onClose={closeModal} />
    </>
  );
};

export const Interactive: Story = {
  args: {
    open: false,
    title: 'Interactive Modal',
    children: (
      <p className="text-sm text-neutral-600">
        Click the button above to open this modal. Press Escape or click the
        overlay to close it.
      </p>
    ),
  },
  render: (args) => <InteractiveTemplate {...args} />,
};
