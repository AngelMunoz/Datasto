-- MIGRONDI:NAME=1736487354482_AddInitialTables.sql
-- MIGRONDI:TIMESTAMP=1736487354482
-- ---------- MIGRONDI:UP ----------
create table users (
    id integer primary key autoincrement,
    name text not null,
    email text not null,
    password text not null,
    created_at datetime default current_timestamp
);

create table roles (
    id integer primary key autoincrement,
    name text not null,
    created_at datetime default current_timestamp
);

create table user_roles (
    id integer primary key autoincrement,
    user_id integer not null,
    role_id integer not null,
    created_at datetime default current_timestamp,
    foreign key (user_id) references users(id),
    foreign key (role_id) references roles(id)
);

create table products (
    id integer primary key autoincrement,
    name text not null,
    price real,
    description text,
    created_at datetime default current_timestamp
);

create table stock_areas (
    id integer primary key autoincrement,
    name text not null,
    created_at datetime default current_timestamp
);

create table stock_area_products (
    id integer primary key autoincrement,
    stock_area_id integer not null,
    product_id integer not null,
    quantity integer not null,
    created_at datetime default current_timestamp,
    updated_at datetime default current_timestamp,
    foreign key (stock_area_id) references stock_areas(id),
    foreign key (product_id) references products(id)
);

-- seed uses, roles, and permissions
insert into users (name, email, password) values ('admin', 'admin@admin', 'admin');
insert into users (name, email, password) values ('user', 'user@user', 'user');

insert into roles (name) values ('admin');
insert into roles (name) values ('user');

insert into user_roles (user_id, role_id) values (1, 1);
insert into user_roles (user_id, role_id) values (2, 2);

-- ---------- MIGRONDI:DOWN ----------
RAISE(ABORT, 'This migration cannot be reverted');




