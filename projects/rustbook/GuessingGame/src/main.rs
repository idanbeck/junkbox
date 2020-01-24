use std::io;
use std::cmp::Ordering;
use rand::Rng;

fn main() {
	println!("Guess the number!");

	let secretNumber = rand::thread_rng().gen_range(1, 101);

	//println!("The secret number is: {}", secretNumber);

	loop {
		println!("Please input your guess");

		let mut guess = String::new();

		io::stdin().read_line(&mut guess).expect("Failed to read line");

		let guess: u32 = match guess.trim().parse() {
			Ok(num) => num,
			Err(_) => {
				println!("Please enter a valid number!");
				continue;
			}, 
		};

		println!("You guessed: {}", guess);

		match guess.cmp(&secretNumber) {
			Ordering::Less => println!("Too small!"),
			Ordering::Greater => println!("Too big!"),
			Ordering::Equal => {
				println!("You Win!");
				break;
			}
		}
	}
}
