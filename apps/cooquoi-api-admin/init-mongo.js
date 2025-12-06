// MongoDB initialization script
// This runs when the container is first created

db = db.getSiblingDB("cooquoi");

// Create application user with readWrite access
db.createUser({
	user: "cooquoiuser",
	pwd: "cooquoipassword",
	roles: [
		{
			role: "readWrite",
			db: "cooquoi",
		},
	],
});

// Create initial collections
db.createCollection("ingredients");

print("MongoDB initialized successfully!");
