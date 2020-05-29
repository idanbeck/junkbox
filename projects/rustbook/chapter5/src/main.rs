
struct color(f32, f32, f32, f32);
struct point(f32, f32, f32, f32);

#[derive(Debug)]
struct Rectangle {
	width: u32,
	height: u32,
}

impl Rectangle {
	fn area(&self) -> u32 {
		return (self.width * self.height);
	}

	fn square(size: u32) -> Rectangle {
		let retRect = Rectangle {width: size, height: size};
		return retRect;
	}
}

struct User {
	username: String,
	email: String,
	signInCount: u64,
	fActive: bool,
}


fn main() {
	let mut user1 = User {
		email: String::from("someone@someone.com"),
		username: String::from("someuser"),
		fActive: true,
		signInCount: 1,
	};

	user1.email = String::from("another@someone.com");

	let user2 = BuildUser(String::from("someother@someother.com"), String::from("someother"));


	let user3 = User { 
		email: String::from("sdkljfk@dskjf.com"),
		username: String::from("sklsdf"),
		..user2
	};

	let black = color(0.0, 0.0, 0.0, 1.0);
	let origin = point(0.0, 0.0, 0.0, 1.0);

	let rect1 = Rectangle{width: 30, height: 50};
	println!("rect1 is {:#?}", rect1);
	println!("area of rect1 is {}", rect1.area());
	
	let sq1 = Rectangle::square(10);
	println!("sq1 is {:#?}", sq1);
}


fn BuildUser(strEmail: String, strUsername: String) -> User {
	return User {
		email: strEmail,
		username: strUsername,
		fActive: true,
		signInCount: 1
	};
}
