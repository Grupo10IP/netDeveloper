-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Versión del servidor:         5.5.5-10.1.36-MariaDB - mariadb.org binary distribution
-- SO del servidor:              Win32
-- HeidiSQL Versión:             8.0.0.4396
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Volcando estructura de base de datos para bbdd
CREATE DATABASE IF NOT EXISTS `bbdd` /*!40100 DEFAULT CHARACTER SET latin1 */;
USE `bbdd`;


-- Volcando estructura para tabla bbdd.boedetails
CREATE TABLE IF NOT EXISTS `boedetails` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `enterprise` varchar(100) NOT NULL,
  `date` varchar(50) NOT NULL,
  `boeid` int(10) unsigned NOT NULL,
  `address` varchar(200) DEFAULT NULL,
  `capital` varchar(20) DEFAULT NULL,
  `socialobject` varchar(1000) DEFAULT NULL,
  `unicpartner` varchar(200) DEFAULT NULL,
  `unicadmin` varchar(200) DEFAULT NULL,
  `registraldata` varchar(1000) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_boedetails_boes` (`boeid`),
  CONSTRAINT `FK_boedetails_boes` FOREIGN KEY (`boeid`) REFERENCES `boes` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- La exportación de datos fue deseleccionada.


-- Volcando estructura para tabla bbdd.boes
CREATE TABLE IF NOT EXISTS `boes` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(100) DEFAULT NULL,
  `anno` int(11) DEFAULT NULL,
  `date` varchar(50) DEFAULT NULL,
  `urlpath` varchar(500) DEFAULT NULL,
  `provinceid` int(10) unsigned NOT NULL,
  `filecontent` longtext,
  PRIMARY KEY (`id`),
  KEY `FK_boes_provinces` (`provinceid`),
  CONSTRAINT `FK_boes_provinces` FOREIGN KEY (`provinceid`) REFERENCES `provinces` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- La exportación de datos fue deseleccionada.


-- Volcando estructura para tabla bbdd.provinces
CREATE TABLE IF NOT EXISTS `provinces` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`),
  UNIQUE KEY `Índice 2` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- La exportación de datos fue deseleccionada.
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
