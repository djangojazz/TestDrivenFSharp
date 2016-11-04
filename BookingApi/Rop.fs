module Ploeh.Samples.Rop

type Result<'TSuccess, 'TFailure> =
  | Success of 'TSuccess
  | Failure of 'TFailure

let bind f x = 
    match x with 
    | Success s -> f s
    | Failure fl -> Failure fl

let map f x = 
    match x with
    | Success s -> Success(f s)
    | Failure fl -> Failure fl