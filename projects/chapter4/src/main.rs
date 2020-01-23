fn main() {
	let mut s1: String = String::from("hello");

	s1.push_str(", world!");

	let mut s2 = s1.clone();

	let len = calc_len(&s1);

	println!("hey {}, {}", s1, len);

	let len = changeString(&mut s2);
	println!("{}, {}", s2, len);

	//	let refNothing = dangle();
	let refSomething = fixedDangle();
	println!("{}", refSomething);

	let i = firstWord(&String::from("this is a string"));
	println!("{}", i);


	let s = String::from("hello world");
	let i = firstWord(&s);
	let strHello = &s[0..i];
	let strWorld = &s[(i + 1)..11];
	println!("{} {}", strHello, strWorld);


	let s = String::from("this is a string");
	println!("The first word of '{}' is '{}'", s, getFirstWord(&s));
}

fn calc_len(s: &String) -> usize {
	s.len()
}

fn changeString(str: &mut String) -> usize {
	str.push_str("supper");
	str.len()
}

/*
fn dangle() -> &String {
	let s = String::from("hi");
	&s
}
*/

fn fixedDangle() -> String {
	let s = String::from("hi");
	return s;
}

fn firstWord(s: &String) -> usize {
	let bytes = s.as_bytes();

	for(i, &item) in bytes.iter().enumerate() {
		if item == b' ' {
			return i;
		}
	}

	return s.len();
}

fn getFirstWord(s: &String) -> &str {
	let bytes = s.as_bytes();
	
	for(i, &item) in bytes.iter().enumerate() {
		if(item == b' ') {
			return &s[0..i];
		}
	}	

	return &s[..];
}
