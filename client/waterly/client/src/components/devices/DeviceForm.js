import React from 'react'
import { Field, reduxForm } from 'redux-form';

//we want helper method so we will use class
class DeviceForm extends React.Component {

    renderError({error, touched}) {
        if(touched && error) {
            return (
                <div className="ui error message">
                    <div className="header">{error}</div>
                </div>
            );
        }
    }

    //we need to use error function because we use this
    renderInput = ({ input, label, meta }) => {
        const className = `field ${meta.error && meta.touched ? 'error': ''}`;
        return (
            <div className={className}>
                <label>{label}</label>
                <input {...input} autoComplete="off"/>
                {this.renderError(meta)}
            </div>
        );
    }

    onSubmit = (formValues) => {
        console.log(formValues)
        //parent component need to send onSubmit func
        this.props.onSubmit(formValues)
    }

    render() {
        return (
            <form onSubmit={this.props.handleSubmit(this.onSubmit)} 
                className="ui form error"
            >
                <Field name="name" component={this.renderInput} label="Enter Name" />
                <Field name="address.city" component={this.renderInput} label="Enter City"/>
                <Field name="address.street" component={this.renderInput} label="Enter Street"/>
                <Field name="address.building_num" component={this.renderInput} label="Enter Building Num"/>
                <Field name="address.apt_num" component={this.renderInput} label="Enter Apt Num"/>
            <button className="ui button primary">Submit</button>
            </form>
        );
    }; 
}

const validate = formValues => {
    const error = {};
    
    if(!formValues.name){
        error.name = 'You must enter a name';
    }

    if(formValues.address) 
    {
        error.address = {}
        if(!formValues.address.city){
            error.address.city = 'You must enter a city';
        }
        if(!formValues.address.street){
            error.address.street = 'You must enter a street';
        }
        if(!formValues.address.building_num){
            error.address.building_num = 'You must enter a building num';
        }
        if(!formValues.address.apt_num){
            error.address.apt_num = 'You must enter a apt num';
        }
    }
    return error;
};

//redux form return a function and we immedetly call it with stream create
export default reduxForm({
    form: 'DeviceForm',
    validate
})(DeviceForm);
