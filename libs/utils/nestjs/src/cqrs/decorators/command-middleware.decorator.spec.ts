import { ICommand } from '@nestjs/cqrs';
import { vi } from 'vitest';

import {
  COMMAND_MIDDLEWARE,
  CommandMiddleware,
} from './command-middleware.decorator';

// Mock command class for testing
class TestCommand implements ICommand {
  constructor(public readonly id: string) {}
}

describe('CommandMiddleware', () => {
  it('should define metadata on the target class', () => {
    // Arrange
    @CommandMiddleware(TestCommand)
    class TestCommandMiddleware {
      process(_: TestCommand): void {
        void _;
      }
    }

    // Act
    const metadata = Reflect.getMetadata(
      COMMAND_MIDDLEWARE,
      TestCommandMiddleware,
    );

    // Assert
    expect(metadata).toBeDefined();
    expect(metadata.type).toBe(TestCommand);
  });

  it('should pass the correct metadata to Reflect.defineMetadata', () => {
    // Arrange
    const defineMetadataSpy = vi.spyOn(Reflect, 'defineMetadata');

    // Act
    @CommandMiddleware(TestCommand)
    class TestCommandMiddleware {
      process(_: TestCommand): void {
        void _;
      }
    }

    // Assert
    expect(defineMetadataSpy).toHaveBeenCalledWith(
      COMMAND_MIDDLEWARE,
      { type: TestCommand },
      TestCommandMiddleware,
    );

    // Clean up
    defineMetadataSpy.mockRestore();
  });
});
