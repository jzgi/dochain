/*
Navicat PGSQL Data Transfer

Source Server         : 106.14.45.109
Source Server Version : 90505
Source Host           : 106.14.45.109:5432
Source Database       : samp
Source Schema         : public

Target Server Type    : PGSQL
Target Server Version : 90505
File Encoding         : 65001

Date: 2017-10-08 10:38:08
*/


-- ----------------------------
-- Sequence structure for orders_id_seq1
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."orders_id_seq1";
CREATE SEQUENCE "public"."orders_id_seq1"
 INCREMENT 1
 MINVALUE 1
 MAXVALUE 9223372036854775807
 START 1
 CACHE 1;

-- ----------------------------
-- Sequence structure for repays_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."repays_id_seq";
CREATE SEQUENCE "public"."repays_id_seq"
 INCREMENT 1
 MINVALUE 1000
 MAXVALUE 9223372036854775807
 START 1293
 CACHE 16;
SELECT setval('"public"."repays_id_seq"', 1293, true);

-- ----------------------------
-- Sequence structure for shops_id_seq
-- ----------------------------
DROP SEQUENCE IF EXISTS "public"."shops_id_seq";
CREATE SEQUENCE "public"."shops_id_seq"
 INCREMENT 1
 MINVALUE 1
 MAXVALUE 9223372036854775807
 START 1
 CACHE 1;
SELECT setval('"public"."shops_id_seq"', 1, true);

-- ----------------------------
-- Table structure for items
-- ----------------------------
DROP TABLE IF EXISTS "public"."items";
CREATE TABLE "public"."items" (
"shopid" int2 NOT NULL,
"name" varchar(20) COLLATE "default" NOT NULL,
"descr" varchar(30) COLLATE "default",
"icon" bytea,
"unit" varchar(4) COLLATE "default",
"price" money,
"min" int2,
"step" int2,
"max" int2,
"customs" varchar(30)[] COLLATE "default",
"status" int2
)
WITH (OIDS=FALSE)

;

-- ----------------------------
-- Table structure for kicks
-- ----------------------------
DROP TABLE IF EXISTS "public"."kicks";
CREATE TABLE "public"."kicks" (
"id" int4,
"wx" varchar(28) COLLATE "default",
"city" varchar(6) COLLATE "default",
"report" varchar(50) COLLATE "default",
"creator" varchar(6) COLLATE "default",
"created" timestamp(6),
"status" int2
)
WITH (OIDS=FALSE)

;

-- ----------------------------
-- Table structure for lessons
-- ----------------------------
DROP TABLE IF EXISTS "public"."lessons";
CREATE TABLE "public"."lessons" (
"id" varchar(2) COLLATE "default" NOT NULL,
"name" varchar(20) COLLATE "default",
"refid" varchar(20) COLLATE "default",
"modified" timestamp(6)
)
WITH (OIDS=FALSE)

;

-- ----------------------------
-- Table structure for orders
-- ----------------------------
DROP TABLE IF EXISTS "public"."orders";
CREATE TABLE "public"."orders" (
"id" int8 DEFAULT nextval('orders_id_seq1'::regclass) NOT NULL,
"shopid" int2,
"shopname" varchar(10) COLLATE "default",
"wx" varchar(28) COLLATE "default",
"name" varchar(10) COLLATE "default",
"tel" varchar(11) COLLATE "default",
"city" varchar(6) COLLATE "default",
"area" varchar(10) COLLATE "default",
"addr" varchar(20) COLLATE "default",
"items" jsonb,
"total" money,
"created" timestamp(6),
"cash" money DEFAULT 0,
"accepted" timestamp(6),
"abortly" varchar(20) COLLATE "default",
"aborted" timestamp(6),
"shipped" timestamp(6),
"note" varchar(20) COLLATE "default",
"status" int2
)
WITH (OIDS=FALSE)

;

-- ----------------------------
-- Table structure for repays
-- ----------------------------
DROP TABLE IF EXISTS "public"."repays";
CREATE TABLE "public"."repays" (
"id" int4 DEFAULT nextval('repays_id_seq'::regclass) NOT NULL,
"shopid" int2,
"shopname" varchar(10) COLLATE "default",
"till" date,
"orders" int4,
"total" money,
"cash" money,
"paid" timestamp(6),
"payer" varchar(6) COLLATE "default",
"status" int2 DEFAULT 0,
"err" varchar(40) COLLATE "default"
)
WITH (OIDS=FALSE)

;

-- ----------------------------
-- Table structure for shops
-- ----------------------------
DROP TABLE IF EXISTS "public"."shops";
CREATE TABLE "public"."shops" (
"id" int2 DEFAULT nextval('shops_id_seq'::regclass) NOT NULL,
"name" varchar(10) COLLATE "default",
"city" varchar(6) COLLATE "default",
"addr" varchar(20) COLLATE "default",
"x" float8,
"y" float8,
"icon" bytea,
"areas" varchar(10)[] COLLATE "default",
"mgrwx" varchar(28) COLLATE "default",
"mgrtel" varchar(11) COLLATE "default",
"mgrname" varchar(10) COLLATE "default",
"oprwx" varchar(28) COLLATE "default",
"oprtel" varchar(11) COLLATE "default",
"oprname" varchar(10) COLLATE "default",
"status" int2
)
WITH (OIDS=FALSE)

;

-- ----------------------------
-- Table structure for users
-- ----------------------------
DROP TABLE IF EXISTS "public"."users";
CREATE TABLE "public"."users" (
"wx" varchar(28) COLLATE "default" NOT NULL,
"name" varchar(10) COLLATE "default",
"tel" varchar(11) COLLATE "default",
"credential" varchar(32) COLLATE "default",
"city" varchar(4) COLLATE "default",
"addr" varchar(20) COLLATE "default",
"oprat" int2,
"opr" int2 DEFAULT 0,
"adm" bool DEFAULT false,
"status" int2 DEFAULT 0 NOT NULL
)
WITH (OIDS=FALSE)

;

-- ----------------------------
-- Alter Sequences Owned By 
-- ----------------------------
ALTER SEQUENCE "public"."orders_id_seq1" OWNED BY "orders"."id";
ALTER SEQUENCE "public"."repays_id_seq" OWNED BY "repays"."id";
ALTER SEQUENCE "public"."shops_id_seq" OWNED BY "shops"."id";

-- ----------------------------
-- Primary Key structure for table items
-- ----------------------------
ALTER TABLE "public"."items" ADD PRIMARY KEY ("shopid", "name");

-- ----------------------------
-- Primary Key structure for table repays
-- ----------------------------
ALTER TABLE "public"."repays" ADD PRIMARY KEY ("id");

-- ----------------------------
-- Primary Key structure for table shops
-- ----------------------------
ALTER TABLE "public"."shops" ADD PRIMARY KEY ("id");
