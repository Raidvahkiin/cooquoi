export abstract class DatetimeProvider {
	abstract now(): Date;
}

export class SystemDatetimeProvider extends DatetimeProvider {
	now(): Date {
		return new Date();
	}
}
