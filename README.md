# MyCode
DROP TABLE IF EXISTS SysUser;

CREATE TABLE SysUser
(
   UserId    VARCHAR(10) PRIMARY KEY,
   UserPw    VARBINARY(50) NOT NULL,
   FullName  VARCHAR(50) NOT NULL,
   Email     VARCHAR(50) NOT NULL,
   UserRole  VARCHAR(10) NOT NULL,
   ATT      INT NOT NULL,
   BTT      INT NOT NULL,
   AHT      INT NOT NULL,
   BHT      INT NOT NULL,
   ACOT     INT NOT NULL,
   BCOT     INT NOT NULL,
   EmailTemp DATETIME NULL,
   EmailHumid DATETIME NULL,
   EmailCO2 DATETIME NULL,
   TimeIntervalTemp INT NOT NULL,
   TimeIntervalHumid INT NOT NULL,
   TimeIntervalCO2 INT NOT NULL
);

INSERT INTO SysUser(UserId, UserPw, FullName, Email, UserRole, ATT, BTT, AHT, BHT, ACOT, BCOT, EmailTemp, EmailHumid, EmailCO2, TimeIntervalTemp, TimeIntervalHumid, TimeIntervalCO2)  VALUES
('dan', HASHBYTES('SHA1', '1232'), 'Daniel Chung', 'Dan@FYP.com', 'admin', 40, 20, 70, 30, 400, 1000, NULL, NULL, NULL, 4, 3, 2),
('earn', HASHBYTES('SHA1', '123'), 'Earnest Lim', 'Earn@xyc.com', 'farmer', 25, 10, 200, 10, 400, 1000, NULL, NULL, NULL, 4, 3, 2),
('eddie', HASHBYTES('SHA1', '123'), 'Eddie Koh', 'Eddie@xyc.com', 'farmer', 40, 30, 200, 10, 400, 1000, NULL, NULL, NULL, 4, 3, 2);


//CO2 Sample Data

INSERT INTO `co2` (`id`, `co2`, `datetime`) VALUES
(11, 321, '2023-07-12 13:52:07'),
(12, 321, '2023-07-12 15:27:10'),
(13, 321, '2023-07-12 15:27:12'),
(14, 321, '2023-07-12 15:27:14'),
(15, 321, '2023-07-12 15:27:15'),
(16, 321, '2023-07-12 15:27:17'),
(17, 321, '2023-07-12 15:27:19'),
(18, 321, '2023-07-12 15:27:20'),
(19, 321, '2023-07-12 15:27:22'),
(20, 321, '2023-07-12 15:27:24'),
(21, 321, '2023-07-12 15:27:25'),
(22, 321, '2023-07-12 15:27:27'),
(23, 348, '2023-07-12 15:27:29'),
(24, 348, '2023-07-12 15:27:30'),
(25, 348, '2023-07-12 15:27:32'),
(26, 348, '2023-07-12 15:27:34'),
(27, 348, '2023-07-12 15:27:36'),
(28, 378, '2023-07-12 15:27:38'),
(29, 378, '2023-07-12 15:27:39'),
(30, 378, '2023-07-12 15:27:41'),
(31, 378, '2023-07-12 15:27:43'),
(32, 378, '2023-07-12 15:27:45'),
(33, 378, '2023-07-12 15:27:46'),
(34, 378, '2023-07-12 15:27:48'),
(35, 378, '2023-07-12 15:27:49'),
(36, 366, '2023-07-12 15:27:51'),
(37, 366, '2023-07-12 15:27:53'),
(38, 366, '2023-07-12 15:27:54'),
(39, 366, '2023-07-12 15:27:56'),
(40, 366, '2023-07-12 15:27:58'),
(41, 366, '2023-07-12 15:28:00'),
(42, 401, '2023-07-12 15:28:01'),
(43, 401, '2023-07-12 15:28:03'),
(44, 401, '2023-07-12 15:28:05'),
(45, 421, '2023-07-12 15:28:07'),
(46, 421, '2023-07-12 15:28:08'),
(47, 421, '2023-07-12 15:28:10'),
(48, 361, '2023-07-12 15:28:15'),
(49, 361, '2023-07-12 15:28:17'),
(50, 361, '2023-07-12 15:28:18'),
(51, 361, '2023-07-12 15:28:20'),
