import { NestFactory } from "@nestjs/core";
import { ConfigService } from "@nestjs/config";
import {
	FastifyAdapter,
	NestFastifyApplication,
} from "@nestjs/platform-fastify";
import { DocumentBuilder, SwaggerModule } from "@nestjs/swagger";
import { AppModule } from "./app.module";

async function bootstrap() {
	const app = await NestFactory.create<NestFastifyApplication>(
		AppModule.register(),
		new FastifyAdapter(),
	);

	const config = new DocumentBuilder()
		.setTitle("Cooquoi Admin API")
		.setDescription("API for managing ingredients and recipes")
		.setVersion("1.0")
		.build();
	const document = SwaggerModule.createDocument(app, config);
	SwaggerModule.setup("api", app, document);

	const configService = app.get(ConfigService);
	const port = configService.get<number>("PORT", 3000);
	const host = configService.get<string>("HOST", "0.0.0.0");

	await app.listen(port, host);
	console.log(`Application is running on: ${await app.getUrl()}`);
	console.log(`Swagger UI available at: ${await app.getUrl()}/api`);
}

bootstrap();
