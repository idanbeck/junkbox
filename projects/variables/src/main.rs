const MAX_POINTS: u32 = 100_000;

fn main() {
	let x = 5;
	println!("The value of x is: {}", x);
	
	let x = x + 1;
	let x = x * 2;
	println!("The value of x is: {}", x);

	println!("val max: {}", MAX_POINTS);


	let guess: u32 = "42".parse().expect("Not a number!");
	println!("guess: {}", guess);

	let x = 2.0;
	println!("x: {}", x);

	let tup: (i32, f64, u8) = (500, 6.4, 1);
	println!("tup: {}", tup.1);


	let a: [i32; 5] = [1, 2, 3, 4, 5];
	println!("a[3]: {}", a[3]);

	someFunction(5, 7);
	
	let y = {
		let x = 3;
		x + 1
	};

	println!("y: {}", y);
	println!("five(): {}", five());


	let fCondition = true;
	let num = if (fCondition) {
		5
	}
	else {
		6
	};

	println!("num: {}", num);

	let mut i = 0;
	let result = loop {
		i += 1;
		if(i == 10) {
			break i * 2;
		}
	};
	
	println!("result: {}", result);

	i = 10;
	while (i != 0) {
		println!("{}!", i);
		i = i - 1;
	};
	println!("lift off"); 


	let arr = [10, 20, 30, 40, 50];
	for element in arr.iter() {
		println!("val element: {}", element);
	}

	for i in (1..10).rev() {
		println!("{}!", i);
	}
	println!("liftoff 2");
}

fn someFunction(x : i32, y : i32) {
	println!("Some function {}, {}", x, y);
}

fn five() -> i32 {
	5
}
