enum IpAddr {
	V4(u8, u8, u8, u8),
	V6(String),
}

enum Message { 
	Quit,
	Move {x: i32, y: i32 },
	Write(String),
	ChangeColor(i32, i32, i32),
	Invalid
}

impl Message{
	fn call(&self) {
		match self {
			Message::Quit => {
				println!("Quit!");
				std::process::exit(0);
			},

			Message::Move{x, y} => {
				println!("Move {}, {}", x, y);
			},

			Message::Write(strMessage) => {
				println!("write: {}", strMessage);
			},

			Message::ChangeColor(r, g, b) => {
				println!("change color ({}, {}, {})", r, g, b);
			},

			_ => {
				println!("invalid");
			},
		}
	}
}

enum Coin { 
	penny, 
	nickel,
	dime,
	quarter
}

fn main() {
	let home = IpAddr::V4(127, 0, 0, 1);
	let loopback = IpAddr::V6(String::from("::1"));

	let m = Message::Write(String::from("hello"));
	m.call();

	let m = Message::Invalid;
	m.call();

	let some_number = Some(5);
	let some_string = Some("a string");

	let absent_number: Option<i32> = None;

	let x: i8 = 5;
	let y: Option<i8> = Some(5);

	// Will error using unwrap below
	//let y: Option<i8> = None;

	// Error:
	//let sum = x + y;

	let sum = x + y.unwrap();
	println!("{}", sum);

	value_in_cents(Coin::penny);

}

//fn route(ip_kind: IpAddrKind) {
//
//}

fn value_in_cents(coin: Coin) -> u8 {
	match coin {
		Coin::penny => {
			println!("lucky penny");
			1
		},
		Coin::nickel => 5,
		Coin::dime => 10,
		Coin::quarter => 25
	}
}