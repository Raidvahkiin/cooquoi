import { InstanceWrapper } from '@nestjs/core/injector/instance-wrapper';
import { ICommand } from '@nestjs/cqrs';
import { vi } from 'vitest';

import { _ICommandMiddlewareHandler } from '../middlewares';

import { COMMAND_MIDDLEWARE } from './command-middleware.decorator';
import { CommandMiddlewareMetadata } from './command-middleware.decorator.metadata';
import {
  _getMiddlewareMetadata,
  getMiddlewareMetadata,
} from './decorator.reflection';

// Mock implementation of a command and middleware handler
class TestCommand implements ICommand {}

class TestMiddlewareHandler implements _ICommandMiddlewareHandler {
  execute(_: ICommand) {
    void _;
    return Promise.resolve();
  }
}

describe('Decorator Reflection', () => {
  describe('_getMiddlewareMetadata', () => {
    it('should return undefined when no metadata is found', () => {
      // Arrange
      const testClass = TestMiddlewareHandler;
      vi.spyOn(Reflect, 'getMetadata').mockReturnValue(undefined);

      // Act
      const result = _getMiddlewareMetadata<TestCommand>(testClass);

      // Assert
      expect(result).toBeUndefined();
      expect(Reflect.getMetadata).toHaveBeenCalledWith(
        COMMAND_MIDDLEWARE,
        testClass,
      );
    });

    it('should return metadata when found', () => {
      // Arrange
      const testClass = TestMiddlewareHandler;

      const mockMetadata: CommandMiddlewareMetadata<TestCommand> = {
        type: TestCommand,
      };

      vi.spyOn(Reflect, 'getMetadata').mockReturnValue(mockMetadata);

      // Act
      const result = _getMiddlewareMetadata<TestCommand>(testClass);

      // Assert
      expect(result).toBe(mockMetadata);
      expect(Reflect.getMetadata).toHaveBeenCalledWith(
        COMMAND_MIDDLEWARE,
        testClass,
      );
    });
  });

  describe('getMiddlewareMetadata', () => {
    it('should return metadata when found', () => {
      // Arrange
      const testInstance = new TestMiddlewareHandler();

      const mockInstanceWrapper = new InstanceWrapper({
        name: 'TestMiddlewareHandler',
        instance: testInstance,
      });

      const mockMetadata: CommandMiddlewareMetadata<ICommand> = {
        type: TestCommand,
      };

      vi.spyOn(Reflect, 'getMetadata').mockReturnValue(mockMetadata);

      // Act
      const result = getMiddlewareMetadata(mockInstanceWrapper);

      // Assert
      expect(result).toBe(mockMetadata);
      expect(Reflect.getMetadata).toHaveBeenCalledWith(
        COMMAND_MIDDLEWARE,
        TestMiddlewareHandler,
      );
    });

    it('should throw error when no metadata is found', () => {
      // Arrange
      const testInstance = new TestMiddlewareHandler();

      const mockInstanceWrapper = new InstanceWrapper({
        name: 'TestMiddlewareHandler',
        instance: testInstance,
      });

      vi.spyOn(Reflect, 'getMetadata').mockReturnValue(undefined);

      // Act & Assert
      expect(() => getMiddlewareMetadata(mockInstanceWrapper)).toThrow(
        'No metadata found for processor TestMiddlewareHandler',
      );
    });
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });
});
