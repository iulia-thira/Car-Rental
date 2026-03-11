import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { Plus, X } from 'lucide-react';
import { listingsApi } from '@/services/apiServices';

const schema = z.object({
  make: z.string().min(1, 'Required'),
  model: z.string().min(1, 'Required'),
  year: z.number().min(1990).max(new Date().getFullYear() + 1),
  description: z.string().min(20, 'Min 20 characters'),
  pricePerDay: z.number().min(1, 'Required'),
  deposit: z.number().min(0),
  latitude: z.number(),
  longitude: z.number(),
  address: z.string().min(1, 'Required'),
  city: z.string().min(1, 'Required'),
  country: z.string().min(1, 'Required'),
  transmission: z.enum(['Manual', 'Automatic']),
  fuelType: z.enum(['Petrol', 'Diesel', 'Electric', 'Hybrid']),
  seats: z.number().min(1).max(9),
  mileageLimit: z.number().optional(),
  features: z.array(z.object({ value: z.string() })),
});

type FormData = z.infer<typeof schema>;

export default function CreateListingPage() {
  const navigate = useNavigate();
  const { register, handleSubmit, control, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { year: 2020, seats: 5, transmission: 'Manual', fuelType: 'Petrol', latitude: 0, longitude: 0, features: [] }
  });

  const { fields, append, remove } = useFieldArray({ control, name: 'features' });

  const mutation = useMutation({
    mutationFn: (data: FormData) => listingsApi.create({
      ...data,
      features: data.features.map(f => f.value).filter(Boolean),
    }),
    onSuccess: () => navigate('/owner'),
  });

  const field = (name: string, label: string, type = 'text', placeholder = '') => (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
      <input {...register(name as any, { valueAsNumber: type === 'number' })} type={type} placeholder={placeholder}
        className="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
      {(errors as any)[name] && <p className="text-red-500 text-xs mt-1">{(errors as any)[name]?.message}</p>}
    </div>
  );

  return (
    <div className="max-w-2xl mx-auto">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">List Your Car</h1>

      <form onSubmit={handleSubmit((d) => mutation.mutate(d))} className="space-y-6">
        <div className="bg-white border border-gray-200 rounded-xl p-6">
          <h2 className="font-semibold text-gray-800 mb-4">Car Details</h2>
          <div className="grid grid-cols-2 gap-4">
            {field('make', 'Make', 'text', 'Toyota')}
            {field('model', 'Model', 'text', 'Corolla')}
            {field('year', 'Year', 'number', '2020')}
            {field('seats', 'Seats', 'number', '5')}
          </div>
          <div className="grid grid-cols-2 gap-4 mt-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Transmission</label>
              <select {...register('transmission')} className="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                <option value="Manual">Manual</option>
                <option value="Automatic">Automatic</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Fuel Type</label>
              <select {...register('fuelType')} className="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                {['Petrol', 'Diesel', 'Electric', 'Hybrid'].map(f => <option key={f} value={f}>{f}</option>)}
              </select>
            </div>
          </div>
          <div className="mt-4">
            <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
            <textarea {...register('description')} rows={3} placeholder="Describe your car, its condition, and any rules..."
              className="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none" />
            {errors.description && <p className="text-red-500 text-xs mt-1">{errors.description.message}</p>}
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-6">
          <h2 className="font-semibold text-gray-800 mb-4">Pricing</h2>
          <div className="grid grid-cols-2 gap-4">
            {field('pricePerDay', 'Price per Day ($)', 'number', '50')}
            {field('deposit', 'Deposit ($)', 'number', '200')}
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-6">
          <h2 className="font-semibold text-gray-800 mb-4">Location</h2>
          <div className="grid grid-cols-2 gap-4">
            {field('address', 'Street Address')}
            {field('city', 'City')}
            {field('country', 'Country')}
            {field('latitude', 'Latitude', 'number')}
            {field('longitude', 'Longitude', 'number')}
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-xl p-6">
          <div className="flex justify-between items-center mb-4">
            <h2 className="font-semibold text-gray-800">Features</h2>
            <button type="button" onClick={() => append({ value: '' })}
              className="flex items-center gap-1 text-blue-600 text-sm font-medium hover:text-blue-700">
              <Plus className="w-4 h-4" /> Add Feature
            </button>
          </div>
          <div className="space-y-2">
            {fields.map((field, i) => (
              <div key={field.id} className="flex gap-2">
                <input {...register(`features.${i}.value`)} placeholder="e.g. GPS, Bluetooth, Baby Seat"
                  className="flex-1 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                <button type="button" onClick={() => remove(i)} className="text-red-400 hover:text-red-600">
                  <X className="w-4 h-4" />
                </button>
              </div>
            ))}
          </div>
        </div>

        {mutation.isError && <p className="text-red-600 text-sm bg-red-50 p-3 rounded-lg">Failed to create listing. Please try again.</p>}

        <div className="flex gap-3">
          <button type="button" onClick={() => navigate('/owner')}
            className="flex-1 border border-gray-300 text-gray-700 py-3 rounded-xl font-medium hover:bg-gray-50">
            Cancel
          </button>
          <button type="submit" disabled={mutation.isPending}
            className="flex-1 bg-blue-600 text-white py-3 rounded-xl font-semibold hover:bg-blue-700 disabled:opacity-60">
            {mutation.isPending ? 'Creating...' : 'Create Listing'}
          </button>
        </div>
      </form>
    </div>
  );
}
