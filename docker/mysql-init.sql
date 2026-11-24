CREATE DATABASE IF NOT EXISTS car_marketplace CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
CREATE DATABASE IF NOT EXISTS car_marketplace_test CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;

GRANT ALL PRIVILEGES ON car_marketplace.* TO 'car_app'@'%';
GRANT ALL PRIVILEGES ON car_marketplace_test.* TO 'car_app'@'%';
FLUSH PRIVILEGES;
